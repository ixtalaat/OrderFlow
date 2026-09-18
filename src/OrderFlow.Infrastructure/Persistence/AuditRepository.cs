using OrderFlow.Application.Common.Auditing;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence;

public sealed class AuditRepository(ApplicationDbContext db) : IAuditRepository
{
    public Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        => db.AuditEntries.AddAsync(entry, cancellationToken).AsTask();
}
