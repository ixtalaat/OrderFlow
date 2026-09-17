using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class NoOpBackgroundJobScheduler : IBackgroundJobScheduler
{
    public void EnqueueOrderNotification(Order order) { }
    public void EnqueueAccountingSynchronization(Order order) { }
}
