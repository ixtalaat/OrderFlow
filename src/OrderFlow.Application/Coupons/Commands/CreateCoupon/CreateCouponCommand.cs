using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Coupons.DTOs;

namespace OrderFlow.Application.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(string Code, decimal DiscountPercentage, decimal MinOrderTotal, DateTime ValidFromUtc, DateTime? ValidToUtc, int? MaxRedemptions) : IRequest<Result<CouponResponse>>;
