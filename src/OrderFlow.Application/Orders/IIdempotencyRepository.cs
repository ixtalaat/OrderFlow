using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders;

public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetAsync(string userId, string key, CancellationToken cancellationToken = default);
    Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);
    Task RemoveAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default);
    Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
