using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetAllOrders;

public sealed record GetAllOrdersQuery(OrderQueryParams QueryParams) : IRequest<Result<PagedList<OrderResponse>>>;
