using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.CompleteOrder;

public sealed record CompleteOrderCommand(int OrderId) : IRequest<Result<OrderResponse>>;
