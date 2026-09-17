using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetCustomerOrders;

public sealed class GetCustomerOrdersQueryHandler(IOrderRepository orders) : IRequestHandler<GetCustomerOrdersQuery, Result<PagedList<OrderResponse>>>
{
    public async Task<Result<PagedList<OrderResponse>>> Handle(GetCustomerOrdersQuery query, CancellationToken ct)
        => Result.Success(await orders.GetCustomerOrdersAsync(query.CustomerId, query.QueryParams, ct));
}
