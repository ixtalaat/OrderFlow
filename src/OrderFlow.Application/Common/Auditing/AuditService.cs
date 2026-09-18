using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Common.Auditing;

public sealed class AuditService(IAuditRepository entries, ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<AuditService> logger) : IAuditService
{
    public Task LogAsync(string action, string entityType, string entityId, string? details = null, CancellationToken cancellationToken = default)
        => entries.AddAsync(AuditEntry.Create(currentUser.UserId, action, entityType, entityId, details), cancellationToken);

    public async Task TrySaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Audit entry could not be persisted.");
        }
    }
}
