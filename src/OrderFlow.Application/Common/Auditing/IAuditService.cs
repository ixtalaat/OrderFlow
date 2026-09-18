namespace OrderFlow.Application.Common.Auditing;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string entityId, string? details = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists staged audit entries without failing the business operation on error.
    /// </summary>
    Task TrySaveAsync(CancellationToken cancellationToken = default);
}
