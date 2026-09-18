using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders;

public interface IIdempotencyService
{
    Task<IdempotencyReplay?> LookupAsync(string userId, string key, CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task RecordAsync(string userId, string key, CreateOrderRequest request, OrderResponse response, CancellationToken cancellationToken = default);
}
