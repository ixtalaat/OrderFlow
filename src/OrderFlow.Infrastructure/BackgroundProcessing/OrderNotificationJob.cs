using Hangfire;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Notifications;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class OrderNotificationJob(IOrderNotificationService notifications, ILogger<OrderNotificationJob> logger)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default)
    {
        await notifications.NotifyStatusChangedAsync(orderId, status, cancellationToken);
        logger.LogInformation("Order notification job completed for order {OrderId} with status {Status}.", orderId, status);
    }
}
