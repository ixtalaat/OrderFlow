using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing;

public interface IPricingService
{
    Task<decimal> GetPriceAsync(int productId, decimal basePrice, CustomerTier tier, CancellationToken cancellationToken = default);
}
