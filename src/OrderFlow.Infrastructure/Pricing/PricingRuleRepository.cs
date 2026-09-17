using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Pricing;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Pricing;

public sealed class PricingRuleRepository(ApplicationDbContext db) : IPricingRuleRepository
{
    public Task<PricingRule?> GetByIdAsync(int id, CancellationToken ct = default) => db.PricingRules.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<PricingRule?> GetActiveAsync(int productId, CustomerTier tier, DateTime now, CancellationToken ct = default)
        => db.PricingRules.Where(x => x.ProductId == productId && x.Tier == tier && x.ValidFromUtc <= now && (x.ValidToUtc == null || now < x.ValidToUtc)).OrderByDescending(x => x.ValidFromUtc).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PricingRule>> GetActiveAsync(IReadOnlyCollection<int> productIds, CustomerTier tier, DateTime now, CancellationToken ct = default)
        => await db.PricingRules.AsNoTracking().Where(x => productIds.Contains(x.ProductId) && x.Tier == tier && x.ValidFromUtc <= now && (x.ValidToUtc == null || now < x.ValidToUtc)).OrderByDescending(x => x.ValidFromUtc).ToListAsync(ct);
    public Task<bool> HasOverlappingRuleAsync(int productId, CustomerTier tier, DateTime from, DateTime? to, int? excludingId = null, CancellationToken ct = default)
        => db.PricingRules.AnyAsync(x => x.ProductId == productId && x.Tier == tier && (!excludingId.HasValue || x.Id != excludingId) && (!to.HasValue || x.ValidFromUtc < to) && (!x.ValidToUtc.HasValue || from < x.ValidToUtc), ct);
    public Task AddAsync(PricingRule rule, CancellationToken ct = default) => db.PricingRules.AddAsync(rule, ct).AsTask();
    public void Remove(PricingRule rule) => db.PricingRules.Remove(rule);
}
