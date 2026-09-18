using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Coupons.Commands.DeleteCoupon;

public sealed class DeleteCouponCommandHandler(ICouponRepository coupons, IUnitOfWork unitOfWork) : IRequestHandler<DeleteCouponCommand, Result>
{
    public async Task<Result> Handle(DeleteCouponCommand command, CancellationToken ct)
    {
        var coupon = await coupons.GetByIdAsync(command.Id, ct);
        if (coupon is null) return Result.Failure(CouponErrors.NotFound);
        coupons.Remove(coupon);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
