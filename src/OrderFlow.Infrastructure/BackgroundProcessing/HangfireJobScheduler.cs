using System.Text.Json;
using Hangfire;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class HangfireJobScheduler(ApplicationDbContext db) : IBackgroundJobScheduler
{
    public void EnqueueOrderNotification(Order order)
        => db.OutboxMessages.Add(OutboxMessage.Create("OrderNotification", JsonSerializer.Serialize(new { status = order.Status.ToString() }), order));

    public void EnqueueAccountingSynchronization(Order order)
        => db.OutboxMessages.Add(OutboxMessage.Create("AccountingSynchronization", JsonSerializer.Serialize(new { }), order));
}
