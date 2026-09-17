using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Strategies;

public sealed class WholesalePricingStrategy : IPricingStrategy
{
    public CustomerTier Tier => CustomerTier.Wholesale;
    public decimal Calculate(decimal basePrice, PricingRule? rule) => decimal.Round(basePrice * (1 - (rule?.DiscountPercentage ?? 0) / 100m), 2, MidpointRounding.AwayFromZero);
}
