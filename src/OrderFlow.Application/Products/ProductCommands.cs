using FluentValidation;
using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Products;

public sealed record CreateProductCommand(string Name, string Description, string Sku, decimal Price, string CategoryName) : IRequest<Result<ProductResponse>>;
public sealed record UpdateProductCommand(int Id, string Name, string Description, string Sku, decimal Price, string CategoryName) : IRequest<Result<ProductResponse>>;
public sealed record ActivateProductCommand(int Id) : IRequest<Result>;
public sealed record DeactivateProductCommand(int Id) : IRequest<Result>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Sku).NotEmpty().Matches("^[A-Za-z0-9][A-Za-z0-9-]{2,49}$");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Sku).NotEmpty().Matches("^[A-Za-z0-9][A-Za-z0-9-]{2,49}$");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
    }
}

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

public sealed class ActivateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<ActivateProductCommand, Result>
{
    public async Task<Result> Handle(ActivateProductCommand command, CancellationToken cancellationToken) { var p = await products.GetByIdAsync(command.Id, cancellationToken); if (p is null) return Result.Failure(ProductErrors.NotFound); p.Activate(); await unitOfWork.SaveChangesAsync(cancellationToken); return Result.Success(); }
}

public sealed class DeactivateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<DeactivateProductCommand, Result>
{
    public async Task<Result> Handle(DeactivateProductCommand command, CancellationToken cancellationToken) { var p = await products.GetByIdAsync(command.Id, cancellationToken); if (p is null) return Result.Failure(ProductErrors.NotFound); p.Deactivate(); await unitOfWork.SaveChangesAsync(cancellationToken); return Result.Success(); }
}
