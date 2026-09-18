using System.Text.Json;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DispatchBatchAsync(stoppingToken); }
            catch (Exception ex) { logger.LogError(ex, "Outbox dispatch failed."); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    internal async Task DispatchBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(50)
            .ToListAsync(ct);

        foreach (var message in messages)
        {
            message.IncrementAttempt();
            var payload = JsonSerializer.Deserialize<JsonElement>(message.Payload);
            var orderId = message.OrderId;
            string jobId = message.MessageType switch
            {
                "OrderNotification" => client.Enqueue<OrderNotificationJob>(job => job.ExecuteAsync(orderId, Enum.Parse<OrderStatus>(payload.GetProperty("status").GetString() ?? string.Empty), CancellationToken.None)),
                "AccountingSynchronization" => client.Enqueue<AccountingSynchronizationJob>(job => job.ExecuteAsync(orderId)),
                _ => throw new InvalidOperationException($"Unknown outbox message type '{message.MessageType}'.")
            };
            message.MarkEnqueued(jobId);
        }

        await db.SaveChangesAsync(ct);
    }
}
