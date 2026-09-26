using FluentValidation;

namespace OrderFlow.Application.Products.Commands.SetProductImage;

public sealed class SetProductImageCommandValidator : AbstractValidator<SetProductImageCommand>
{
    public const long MaxBytes = 5 * 1024 * 1024;

    public SetProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.Length).GreaterThan(0).LessThanOrEqualTo(MaxBytes);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(100);
    }
}
