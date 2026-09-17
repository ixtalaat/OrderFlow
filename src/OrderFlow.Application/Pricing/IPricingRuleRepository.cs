using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing;

public interface IPricingRuleRepository
{
    Task<PricingRule?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PricingRule?> GetActiveAsync(int productId, CustomerTier tier, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingRuleAsync(int productId, CustomerTier tier, DateTime validFromUtc, DateTime? validToUtc, int? excludingId = null, CancellationToken cancellationToken = default);
    Task AddAsync(PricingRule rule, CancellationToken cancellationToken = default);
    void Remove(PricingRule rule);
}
