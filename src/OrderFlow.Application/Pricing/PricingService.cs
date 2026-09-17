using OrderFlow.Application.Pricing.Strategies;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing;

public sealed class PricingService(IPricingRuleRepository rules, PricingStrategyResolver resolver) : IPricingService
{
    public async Task<decimal> GetPriceAsync(int productId, decimal basePrice, CustomerTier tier, CancellationToken cancellationToken = default)
    {
        var rule = await rules.GetActiveAsync(productId, tier, DateTime.UtcNow, cancellationToken);
        return resolver.Resolve(tier).Calculate(basePrice, rule);
    }
}
