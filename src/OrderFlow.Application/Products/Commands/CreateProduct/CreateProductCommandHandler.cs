using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Products;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var sku = command.Sku.Trim().ToUpperInvariant();
        if (await products.ExistsBySkuAsync(sku, cancellationToken: cancellationToken)) return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku);
        var category = await products.GetCategoryByNameAsync(command.CategoryName.Trim(), cancellationToken);
        if (category is null) { category = Category.Create(command.CategoryName); await products.AddCategoryAsync(category, cancellationToken); await unitOfWork.SaveChangesAsync(cancellationToken); }
        var product = Product.Create(command.Name, command.Description, sku, command.Price, category.Id);
        await products.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new ProductResponse(product.Id, product.Name, product.Description, product.Sku, product.Price, category.Id, category.Name, product.IsActive, product.CreatedAtUtc, product.UpdatedAtUtc));
    }
}
