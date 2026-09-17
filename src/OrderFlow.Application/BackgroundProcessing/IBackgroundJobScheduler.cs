using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.BackgroundProcessing;

public interface IBackgroundJobScheduler
{
    string EnqueueOrderNotification(int orderId, OrderStatus status);
    string EnqueueAccountingSynchronization(int orderId);
}
