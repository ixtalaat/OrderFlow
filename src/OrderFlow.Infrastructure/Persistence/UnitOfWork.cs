using System.Data;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;

namespace OrderFlow.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork, ITransactionalUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException("The data was changed by another request.");
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        // The execution strategy wraps the whole transaction so transient
        // retries (EnableRetryOnFailure) re-execute it as a retriable unit
        // instead of rejecting user-initiated transactions.
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                await operation();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException("The data was changed by another request.");
        }
    }

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                var result = await operation();
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException("The data was changed by another request.");
        }
    }
}
