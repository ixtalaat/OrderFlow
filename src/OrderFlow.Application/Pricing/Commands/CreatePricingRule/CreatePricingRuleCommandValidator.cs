using FluentValidation;

namespace OrderFlow.Application.Pricing.Commands.CreatePricingRule;

public sealed class CreatePricingRuleCommandValidator : AbstractValidator<CreatePricingRuleCommand>
{
    public CreatePricingRuleCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x).Must(x => !x.ValidToUtc.HasValue || x.ValidToUtc.Value > x.ValidFromUtc).WithMessage("Validity end must be after validity start.");
    }
}
