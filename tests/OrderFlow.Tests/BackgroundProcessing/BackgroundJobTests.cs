using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderFlow.Application.Notifications;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.BackgroundProcessing;

namespace OrderFlow.Tests.BackgroundProcessing;

public sealed class BackgroundJobTests
{
    [Fact]
    public async Task Notification_Job_Should_Execute_Successfully()
    {
        var notifications = Substitute.For<IOrderNotificationService>();
        var job = new OrderNotificationJob(notifications, NullLogger<OrderNotificationJob>.Instance);
        await job.ExecuteAsync(42, OrderStatus.Submitted);
        await notifications.Received(1).NotifyStatusChangedAsync(42, OrderStatus.Submitted, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Jobs_Should_Configure_Three_Retries()
    {
        var method = typeof(OrderNotificationJob).GetMethod(nameof(OrderNotificationJob.ExecuteAsync));
        var retry = method!.GetCustomAttributes(typeof(AutomaticRetryAttribute), false).Cast<AutomaticRetryAttribute>().Single();
        retry.Attempts.Should().Be(3);
    }
}
