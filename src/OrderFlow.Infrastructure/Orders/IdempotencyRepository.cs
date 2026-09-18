using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Orders;

public sealed class IdempotencyRepository(ApplicationDbContext db) : IIdempotencyRepository
{
    public Task<IdempotencyKey?> GetAsync(string userId, string key, CancellationToken cancellationToken = default)
        => db.IdempotencyKeys.FirstOrDefaultAsync(x => x.UserId == userId && x.Key == key, cancellationToken);

    public Task AddAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
        => db.IdempotencyKeys.AddAsync(idempotencyKey, cancellationToken).AsTask();

    public Task RemoveAsync(IdempotencyKey idempotencyKey, CancellationToken cancellationToken = default)
    {
        db.IdempotencyKeys.Remove(idempotencyKey);
        return Task.CompletedTask;
    }

    public async Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var keys = await db.IdempotencyKeys.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        db.IdempotencyKeys.RemoveRange(keys);
    }
}
