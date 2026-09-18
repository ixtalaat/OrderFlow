using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Coupons.DTOs;

namespace OrderFlow.Application.Coupons.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(int Id, decimal DiscountPercentage, decimal MinOrderTotal, DateTime ValidFromUtc, DateTime? ValidToUtc, int? MaxRedemptions) : IRequest<Result<CouponResponse>>;
