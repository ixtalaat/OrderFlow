using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Coupons.Commands.CreateCoupon;
using OrderFlow.Application.Coupons.DTOs;

namespace OrderFlow.Application.Coupons.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandHandler(ICouponRepository coupons, IUnitOfWork unitOfWork) : IRequestHandler<UpdateCouponCommand, Result<CouponResponse>>
{
    public async Task<Result<CouponResponse>> Handle(UpdateCouponCommand command, CancellationToken ct)
    {
        var coupon = await coupons.GetByIdAsync(command.Id, ct);
        if (coupon is null) return Result.Failure<CouponResponse>(CouponErrors.NotFound);
        coupon.Update(command.DiscountPercentage, command.MinOrderTotal, command.ValidFromUtc, command.ValidToUtc, command.MaxRedemptions);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(CreateCouponCommandHandler.ToResponse(coupon));
    }
}
