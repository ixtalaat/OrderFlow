using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Queries.GetOrder;

public sealed class GetOrderQueryHandler(IOrderRepository orders) : IRequestHandler<GetOrderQuery, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(GetOrderQuery query, CancellationToken ct)
    {
        var order = await orders.GetResponseByIdAsync(query.OrderId, ct);
        return order is null ? Result.Failure<OrderResponse>(OrderErrors.NotFound) : Result.Success(order);
    }
}
