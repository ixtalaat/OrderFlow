using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(int OrderId) : IRequest<Result<OrderResponse>>;
