using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(int CustomerId, IReadOnlyList<CreateOrderItemRequest> Items, string? CouponCode = null) : IRequest<Result<OrderResponse>>;
