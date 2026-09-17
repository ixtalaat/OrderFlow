namespace OrderFlow.Application.BackgroundProcessing;

public interface IBackgroundJobScheduler
{
    string EnqueueOrderNotification(int orderId);
    string EnqueueAccountingSynchronization(int orderId);
}
