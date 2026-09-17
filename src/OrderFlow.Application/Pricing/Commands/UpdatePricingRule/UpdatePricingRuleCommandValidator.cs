using FluentValidation;

namespace OrderFlow.Application.Pricing.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleCommandValidator : AbstractValidator<UpdatePricingRuleCommand>
{
    public UpdatePricingRuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x).Must(x => !x.ValidToUtc.HasValue || x.ValidToUtc.Value > x.ValidFromUtc);
    }
}
