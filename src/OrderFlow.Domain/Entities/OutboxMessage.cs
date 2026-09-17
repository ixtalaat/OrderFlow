namespace OrderFlow.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    private OutboxMessage(string messageType, string payload, Order order)
    {
        MessageType = messageType;
        Payload = payload;
        Order = order;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public long Id { get; private set; }
    public string MessageType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public string? HangfireJobId { get; private set; }
    public int Attempts { get; private set; }

    public static OutboxMessage Create(string messageType, string payload, Order order)
        => new(messageType, payload, order);

    public void MarkEnqueued(string jobId)
    {
        HangfireJobId = jobId;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public void IncrementAttempt() => Attempts++;
}
