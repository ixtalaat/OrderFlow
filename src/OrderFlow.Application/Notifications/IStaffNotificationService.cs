namespace OrderFlow.Application.Notifications;

public interface IStaffNotificationService
{
    Task NotifyLowStockAsync(int productId, int availableQuantity, CancellationToken cancellationToken = default);
}
