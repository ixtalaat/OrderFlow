using Hangfire;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class OrderNotificationJob(ILogger<OrderNotificationJob> logger)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task ExecuteAsync(int orderId)
    {
        logger.LogInformation("Order notification job completed for order {OrderId}.", orderId);
        return Task.CompletedTask;
    }
}
