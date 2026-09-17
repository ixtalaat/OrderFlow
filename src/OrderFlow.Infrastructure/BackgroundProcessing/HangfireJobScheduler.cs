using Hangfire;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class HangfireJobScheduler(IBackgroundJobClient client) : IBackgroundJobScheduler
{
    public string EnqueueOrderNotification(int orderId, OrderStatus status) => client.Enqueue<OrderNotificationJob>(job => job.ExecuteAsync(orderId, status, CancellationToken.None));
    public string EnqueueAccountingSynchronization(int orderId) => client.Enqueue<AccountingSynchronizationJob>(job => job.ExecuteAsync(orderId));
}
