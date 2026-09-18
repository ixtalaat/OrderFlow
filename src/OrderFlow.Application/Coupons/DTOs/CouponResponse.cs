namespace OrderFlow.Application.Coupons.DTOs;

public sealed record CouponResponse(int Id, string Code, decimal DiscountPercentage, decimal MinOrderTotal, DateTime ValidFromUtc, DateTime? ValidToUtc, int? MaxRedemptions, int TimesRedeemed, bool IsActive);
