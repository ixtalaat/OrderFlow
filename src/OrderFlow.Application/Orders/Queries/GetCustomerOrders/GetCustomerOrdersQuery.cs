using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetCustomerOrders;

public sealed record GetCustomerOrdersQuery(int CustomerId, OrderQueryParams QueryParams) : IRequest<Result<PagedList<OrderResponse>>>;
