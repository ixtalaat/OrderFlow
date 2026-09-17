using OrderFlow.Application.BackgroundProcessing;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class NoOpBackgroundJobScheduler : IBackgroundJobScheduler
{
    public string EnqueueOrderNotification(int orderId) => string.Empty;
    public string EnqueueAccountingSynchronization(int orderId) => string.Empty;
}
