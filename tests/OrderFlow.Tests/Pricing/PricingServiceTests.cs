using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Pricing.Strategies;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Pricing;

public sealed class PricingServiceTests
{
    private readonly IPricingRuleRepository _rules = Substitute.For<IPricingRuleRepository>();
    private readonly PricingService _service;

    public PricingServiceTests()
    {
        var strategies = new IPricingStrategy[]
        {
            new RegularPricingStrategy(),
            new WholesalePricingStrategy(),
            new VipPricingStrategy()
        };
        _service = new PricingService(_rules, new PricingStrategyResolver(strategies));
    }

    [Fact]
    public async Task Should_Return_Base_Price_When_No_Rule_Applies()
    {
        _rules.GetActiveAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CustomerTier>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PricingRule>());

        var prices = await _service.GetPricesAsync(
            new Dictionary<int, decimal> { [1] = 100m }, CustomerTier.Wholesale);

        prices[1].Should().Be(100m);
    }

    [Fact]
    public async Task Should_Apply_Discount_When_Rule_Applies()
    {
        var rule = PricingRule.Create(1, CustomerTier.Wholesale, 10, DateTime.UtcNow.AddDays(-1), null);
        _rules.GetActiveAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CustomerTier>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new[] { rule });

        var prices = await _service.GetPricesAsync(
            new Dictionary<int, decimal> { [1] = 100m }, CustomerTier.Wholesale);

        prices[1].Should().Be(90m);
    }

    [Fact]
    public async Task Should_Price_Each_Product_With_Its_Own_Rule()
    {
        var rule = PricingRule.Create(2, CustomerTier.Vip, 20, DateTime.UtcNow.AddDays(-1), null);
        _rules.GetActiveAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CustomerTier>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new[] { rule });

        var prices = await _service.GetPricesAsync(
            new Dictionary<int, decimal> { [1] = 100m, [2] = 200m }, CustomerTier.Vip);

        prices[1].Should().Be(100m);
        prices[2].Should().Be(160m);
    }
}
