using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using DomainInventory = OrderFlow.Domain.Entities.Inventory;

namespace OrderFlow.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductRepository products, IInventoryRepository inventories, IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var transactional = unitOfWork as ITransactionalUnitOfWork;
        ProductResponse? response = null;
        async Task Work()
        {
            var sku = command.Sku.Trim().ToUpperInvariant();
            if (await products.ExistsBySkuAsync(sku, cancellationToken: ct)) return;
            var category = await products.GetCategoryByNameAsync(command.CategoryName.Trim(), ct);
            if (category is null)
            {
                category = Category.Create(command.CategoryName);
                await products.AddCategoryAsync(category, ct);
                await unitOfWork.SaveChangesAsync(ct);
            }
            var product = Product.Create(command.Name, command.Description, sku, command.Price, category.Id);
            await products.AddAsync(product, ct);
            await unitOfWork.SaveChangesAsync(ct);
            await inventories.AddAsync(DomainInventory.Create(product.Id), ct);
            await unitOfWork.SaveChangesAsync(ct);
            response = new ProductResponse(product.Id, product.Name, product.Description, product.Sku, product.Price, category.Id, category.Name, product.IsActive, product.CreatedAtUtc, product.UpdatedAtUtc);
        }

        if (transactional is null) await Work();
        else await transactional.ExecuteInTransactionAsync(Work, ct);
        if (response is null) return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku);
        return Result.Success(response);
    }
}
