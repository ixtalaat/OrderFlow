using FluentAssertions;
using OrderFlow.Application.Pricing.Strategies;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Pricing;

public sealed class PricingStrategyTests
{
    [Theory]
    [InlineData(typeof(RegularPricingStrategy), 100, 0, 100)]
    [InlineData(typeof(WholesalePricingStrategy), 100, 10, 90)]
    [InlineData(typeof(VipPricingStrategy), 100, 20, 80)]
    public void Strategy_Should_Calculate_Price(Type strategyType, decimal basePrice, decimal discount, decimal expected)
    {
        var strategy = (IPricingStrategy)Activator.CreateInstance(strategyType)!;
        var rule = PricingRule.Create(1, strategy.Tier, discount, DateTime.UtcNow.AddMinutes(-1), null);
        strategy.Calculate(basePrice, rule).Should().Be(expected);
    }
}
