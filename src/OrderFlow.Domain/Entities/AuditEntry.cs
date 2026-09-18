namespace OrderFlow.Domain.Entities;

public sealed class AuditEntry
{
    private AuditEntry() { }

    private AuditEntry(string? actorUserId, string action, string entityType, string entityId, string? details)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (string.IsNullOrWhiteSpace(entityId)) throw new ArgumentException("Entity id is required.", nameof(entityId));
        ActorUserId = actorUserId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Details = details;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public string? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static AuditEntry Create(string? actorUserId, string action, string entityType, string entityId, string? details = null)
        => new(actorUserId, action, entityType, entityId, details);
}
