using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Strategies;

public interface IPricingStrategy
{
    CustomerTier Tier { get; }
    decimal Calculate(decimal basePrice, PricingRule? rule);
}
