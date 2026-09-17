using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Products;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.Id, cancellationToken);
        if (product is null) return Result.Failure<ProductResponse>(ProductErrors.NotFound);
        var sku = command.Sku.Trim().ToUpperInvariant();
        if (await products.ExistsBySkuAsync(sku, command.Id, cancellationToken)) return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku);
        var category = await products.GetCategoryByNameAsync(command.CategoryName.Trim(), cancellationToken);
        if (category is null) { category = Category.Create(command.CategoryName); await products.AddCategoryAsync(category, cancellationToken); }
        product.UpdateDetails(command.Name, command.Description, sku, command.Price, category.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new ProductResponse(product.Id, product.Name, product.Description, product.Sku, product.Price, category.Id, category.Name, product.IsActive, product.CreatedAtUtc, product.UpdatedAtUtc));
    }
}
