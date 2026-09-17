using Hangfire;
using OrderFlow.Application.BackgroundProcessing;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class HangfireJobScheduler(IBackgroundJobClient client) : IBackgroundJobScheduler
{
    public string EnqueueOrderNotification(int orderId) => client.Enqueue<OrderNotificationJob>(job => job.ExecuteAsync(orderId));
    public string EnqueueAccountingSynchronization(int orderId) => client.Enqueue<AccountingSynchronizationJob>(job => job.ExecuteAsync(orderId));
}
