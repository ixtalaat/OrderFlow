using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Coupons.Commands.DeleteCoupon;

public sealed record DeleteCouponCommand(int Id) : IRequest<Result>;
