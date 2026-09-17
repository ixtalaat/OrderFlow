using FluentValidation;

namespace OrderFlow.Application.Products.Commands.CreateProduct;

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
