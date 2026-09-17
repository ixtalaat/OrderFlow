using OrderFlow.Application.Pricing.Strategies;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing;

public sealed class PricingService(IPricingRuleRepository rules, PricingStrategyResolver resolver) : IPricingService
{
    public async Task<decimal> GetPriceAsync(int productId, decimal basePrice, CustomerTier tier, CancellationToken cancellationToken = default)
    {
        var prices = await GetPricesAsync(new Dictionary<int, decimal> { [productId] = basePrice }, tier, cancellationToken);
        return prices[productId];
    }

    public async Task<IReadOnlyDictionary<int, decimal>> GetPricesAsync(IReadOnlyDictionary<int, decimal> basePrices, CustomerTier tier, CancellationToken cancellationToken = default)
    {
        var rulesByProduct = (await rules.GetActiveAsync(basePrices.Keys.ToArray(), tier, DateTime.UtcNow, cancellationToken))
            .GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.First());
        var strategy = resolver.Resolve(tier);
        return basePrices.ToDictionary(x => x.Key, x => strategy.Calculate(x.Value, rulesByProduct.GetValueOrDefault(x.Key)));
    }
}
