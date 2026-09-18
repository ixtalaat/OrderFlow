namespace OrderFlow.Application.Coupons.DTOs;

public sealed record CouponRequest(string Code, decimal DiscountPercentage, decimal MinOrderTotal, DateTime ValidFromUtc, DateTime? ValidToUtc, int? MaxRedemptions);
