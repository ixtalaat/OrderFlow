namespace OrderFlow.Application.Common.Persistence;

public interface ITransactionalUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
    Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
}
