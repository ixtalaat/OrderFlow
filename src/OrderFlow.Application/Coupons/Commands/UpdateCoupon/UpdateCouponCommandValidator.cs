using FluentValidation;

namespace OrderFlow.Application.Coupons.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.MinOrderTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxRedemptions).GreaterThanOrEqualTo(0).When(x => x.MaxRedemptions.HasValue);
        RuleFor(x => x).Must(x => !x.ValidToUtc.HasValue || x.ValidToUtc.Value > x.ValidFromUtc).WithMessage("Validity end must be after validity start.");
    }
}
