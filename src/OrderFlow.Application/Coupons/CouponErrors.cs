using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Coupons;

public static class CouponErrors
{
    public static readonly Error NotFound = new("Coupon.NotFound", "Coupon not found.", StatusCodes.Status404NotFound);
    public static readonly Error DuplicateCode = new("Coupon.DuplicateCode", "Coupon code is already in use.", StatusCodes.Status409Conflict);
}
