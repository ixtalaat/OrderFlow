using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Infrastructure.BackgroundProcessing;

namespace OrderFlow.Tests.BackgroundProcessing;

public sealed class BackgroundJobTests
{
    [Fact]
    public async Task Notification_Job_Should_Execute_Successfully()
    {
        var job = new OrderNotificationJob(NullLogger<OrderNotificationJob>.Instance);
        await job.ExecuteAsync(42);
    }

    [Fact]
    public void Jobs_Should_Configure_Three_Retries()
    {
        var method = typeof(OrderNotificationJob).GetMethod(nameof(OrderNotificationJob.ExecuteAsync));
        var retry = method!.GetCustomAttributes(typeof(AutomaticRetryAttribute), false).Cast<AutomaticRetryAttribute>().Single();
        retry.Attempts.Should().Be(3);
    }
}
