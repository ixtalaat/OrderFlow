using Microsoft.Extensions.Logging;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Notifications;

public sealed class OrderNotificationService(IOrderRepository orders, ICustomerRepository customers, IEmailSender emailSender, OrderNotificationTemplate template, ILogger<OrderNotificationService> logger) : IOrderNotificationService
{
    public async Task NotifyStatusChangedAsync(int orderId, OrderStatus status, CancellationToken ct = default)
    {
        var order = await orders.GetResponseByIdAsync(orderId, ct) ?? throw new InvalidOperationException($"Order {orderId} was not found.");
        var customer = await customers.GetResponseByIdAsync(order.CustomerId, ct) ?? throw new InvalidOperationException($"Customer {order.CustomerId} was not found.");
        var message = template.Create(status, orderId);
        logger.LogInformation("Sending order status notification for order {OrderId} to {Email}.", orderId, customer.Email);
        await emailSender.SendAsync(customer.Email, message.Subject, message.Body, ct);
    }
}
