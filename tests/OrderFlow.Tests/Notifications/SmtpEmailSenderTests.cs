using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderFlow.Infrastructure.Notifications;

namespace OrderFlow.Tests.Notifications;

public sealed class SmtpEmailSenderTests
{
    [Fact]
    public async Task Should_Skip_Delivery_When_Disabled()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new EmailOptions { Enabled = false }),
            NullLogger<SmtpEmailSender>.Instance);

        var action = () => sender.SendAsync("user@example.com", "Subject", "Body");

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_Throw_When_Smtp_Host_Is_Unreachable()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new EmailOptions
            {
                Enabled = true,
                Host = "localhost",
                Port = 12525,
                From = "no-reply@example.com",
                UserName = "user",
                Password = "secret"
            }),
            NullLogger<SmtpEmailSender>.Instance);

        var action = () => sender.SendAsync("user@example.com", "Subject", "Body");

        await action.Should().ThrowAsync<Exception>();
    }
}
