using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.BackgroundProcessing;

public interface IBackgroundJobScheduler
{
    void EnqueueOrderNotification(Order order);
    void EnqueueAccountingSynchronization(Order order);
}
