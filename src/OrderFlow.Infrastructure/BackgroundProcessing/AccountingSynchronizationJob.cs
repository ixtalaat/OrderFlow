using Hangfire;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class AccountingSynchronizationJob(ILogger<AccountingSynchronizationJob> logger)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public Task ExecuteAsync(int orderId)
    {
        logger.LogInformation("Accounting synchronization job completed for order {OrderId}.", orderId);
        return Task.CompletedTask;
    }
}
