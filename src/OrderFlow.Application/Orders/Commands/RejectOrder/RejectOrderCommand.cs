using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.RejectOrder;

public sealed record RejectOrderCommand(int OrderId) : IRequest<Result<OrderResponse>>;
