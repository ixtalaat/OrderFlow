using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.ProcessOrder;

public sealed record ProcessOrderCommand(int OrderId) : IRequest<Result<OrderResponse>>;
