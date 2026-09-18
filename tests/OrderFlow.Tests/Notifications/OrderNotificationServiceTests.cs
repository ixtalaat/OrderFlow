using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Notifications;

namespace OrderFlow.Tests.Notifications;

public sealed class OrderNotificationServiceTests
{
    [Fact]
    public async Task Should_Send_Status_Email_To_Customer()
    {
        var orders = Substitute.For<IOrderRepository>();
        var customers = Substitute.For<ICustomerRepository>();
        var emailSender = Substitute.For<IEmailSender>();
        var service = new OrderNotificationService(
            orders, customers, emailSender, new OrderNotificationTemplate(), NullLogger<OrderNotificationService>.Instance);

        var order = new OrderResponse(42, 7, OrderStatus.Confirmed, 20m, DateTime.UtcNow,
            new[] { new OrderItemResponse(3, "Product", 2, 10m, 20m) },
            AccountingSyncStatus.Pending, null, 0, null, null);
        var customer = new CustomerResponse(7, "user-1", "Jane", "jane@example.com", "+123", "Address", true, DateTime.UtcNow, null);
        orders.GetResponseByIdAsync(42, Arg.Any<CancellationToken>()).Returns(order);
        customers.GetResponseByIdAsync(7, Arg.Any<CancellationToken>()).Returns(customer);

        await service.NotifyStatusChangedAsync(42, OrderStatus.Confirmed);

        await emailSender.Received(1).SendAsync(
            "jane@example.com",
            Arg.Is<string>(s => s.Contains("42")),
            Arg.Is<string>(b => b.Contains("Confirmed")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_When_Order_Is_Missing()
    {
        var service = new OrderNotificationService(
            Substitute.For<IOrderRepository>(),
            Substitute.For<ICustomerRepository>(),
            Substitute.For<IEmailSender>(),
            new OrderNotificationTemplate(),
            NullLogger<OrderNotificationService>.Instance);

        var action = () => service.NotifyStatusChangedAsync(999, OrderStatus.Confirmed);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
