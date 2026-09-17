using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class NoOpBackgroundJobScheduler : IBackgroundJobScheduler
{
    public string EnqueueOrderNotification(int orderId, OrderStatus status) => string.Empty;
    public string EnqueueAccountingSynchronization(int orderId) => string.Empty;
}
