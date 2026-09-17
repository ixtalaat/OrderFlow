using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Notifications;

public interface IOrderNotificationService
{
    Task NotifyStatusChangedAsync(int orderId, OrderStatus status, CancellationToken cancellationToken = default);
}
