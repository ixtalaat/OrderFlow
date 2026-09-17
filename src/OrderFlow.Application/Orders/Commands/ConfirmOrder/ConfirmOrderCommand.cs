using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand(int OrderId) : IRequest<Result<OrderResponse>>;
