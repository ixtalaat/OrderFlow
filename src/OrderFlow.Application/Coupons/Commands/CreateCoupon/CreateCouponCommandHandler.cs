using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Coupons.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler(ICouponRepository coupons, IUnitOfWork unitOfWork) : IRequestHandler<CreateCouponCommand, Result<CouponResponse>>
{
    public async Task<Result<CouponResponse>> Handle(CreateCouponCommand command, CancellationToken ct)
    {
        if (await coupons.GetByCodeAsync(command.Code, ct) is not null) return Result.Failure<CouponResponse>(CouponErrors.DuplicateCode);
        var coupon = Coupon.Create(command.Code, command.DiscountPercentage, command.MinOrderTotal, command.ValidFromUtc, command.ValidToUtc, command.MaxRedemptions);
        await coupons.AddAsync(coupon, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(ToResponse(coupon));
    }

    internal static CouponResponse ToResponse(Coupon coupon)
        => new(coupon.Id, coupon.Code, coupon.DiscountPercentage, coupon.MinOrderTotal, coupon.ValidFromUtc, coupon.ValidToUtc, coupon.MaxRedemptions, coupon.TimesRedeemed, coupon.IsActive);
}
