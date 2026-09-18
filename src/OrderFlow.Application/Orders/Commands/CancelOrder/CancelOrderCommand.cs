using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(int OrderId, int CustomerId) : IRequest<Result<OrderResponse>>;
