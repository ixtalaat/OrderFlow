using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Notifications;

public sealed class OrderNotificationTemplate
{
    public (string Subject, string Body) Create(OrderStatus status, int orderId)
        => ($"Order #{orderId} status updated", $"Your order #{orderId} status is now {status}.");
}
