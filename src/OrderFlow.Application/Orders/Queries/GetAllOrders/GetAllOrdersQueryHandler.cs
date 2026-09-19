using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryHandler(IOrderRepository orders) : IRequestHandler<GetAllOrdersQuery, Result<PagedList<OrderResponse>>>
{
    public async Task<Result<PagedList<OrderResponse>>> Handle(GetAllOrdersQuery query, CancellationToken ct)
        => Result.Success(await orders.GetAllOrdersAsync(query.QueryParams, ct));
}
