using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Pricing;

public sealed class PricingRuleTests
{
    private static readonly DateTime From = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_Should_Reject_Invalid_Inputs()
    {
        FluentActions.Invoking(() => PricingRule.Create(0, CustomerTier.Regular, 10, From, To))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => PricingRule.Create(1, CustomerTier.Regular, -1, From, To))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => PricingRule.Create(1, CustomerTier.Regular, 101, From, To))
            .Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => PricingRule.Create(1, CustomerTier.Regular, 10, To, From))
            .Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(365, true)]
    public void IsValidAt_Should_Respect_Open_Ended_Window(int daysOffset, bool expected)
    {
        var rule = PricingRule.Create(1, CustomerTier.Regular, 10, From, null);
        rule.IsValidAt(From.AddDays(daysOffset)).Should().Be(expected);
    }

    [Fact]
    public void IsValidAt_Should_Respect_Closed_Window()
    {
        var rule = PricingRule.Create(1, CustomerTier.Regular, 10, From, To);
        rule.IsValidAt(From.AddDays(-1)).Should().BeFalse();
        rule.IsValidAt(From).Should().BeTrue();
        rule.IsValidAt(To.AddDays(-1)).Should().BeTrue();
        rule.IsValidAt(To).Should().BeFalse();
    }
}
