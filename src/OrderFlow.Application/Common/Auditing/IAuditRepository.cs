using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Common.Auditing;

public interface IAuditRepository
{
    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
