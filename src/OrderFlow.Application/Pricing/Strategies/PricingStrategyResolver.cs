using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Strategies;

public sealed class PricingStrategyResolver(IEnumerable<IPricingStrategy> strategies)
{
    public IPricingStrategy Resolve(CustomerTier tier) => strategies.FirstOrDefault(x => x.Tier == tier) ?? throw new InvalidOperationException($"No pricing strategy registered for {tier}.");
}
