using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Strategies;

public sealed class RegularPricingStrategy : IPricingStrategy
{
    public CustomerTier Tier => CustomerTier.Regular;
    public decimal Calculate(decimal basePrice, PricingRule? rule) => Apply(basePrice, rule);
    private static decimal Apply(decimal basePrice, PricingRule? rule) => decimal.Round(basePrice * (1 - (rule?.DiscountPercentage ?? 0) / 100m), 2, MidpointRounding.AwayFromZero);
}
