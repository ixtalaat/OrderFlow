namespace OrderFlow.Domain.Entities;

public sealed class PricingRule
{
    private PricingRule() { }
    private PricingRule(int productId, CustomerTier tier, decimal discountPercentage, DateTime validFromUtc, DateTime? validToUtc)
    {
        SetValues(productId, tier, discountPercentage, validFromUtc, validToUtc);
    }

    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public CustomerTier Tier { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }

    public static PricingRule Create(int productId, CustomerTier tier, decimal discountPercentage, DateTime validFromUtc, DateTime? validToUtc)
        => new(productId, tier, discountPercentage, validFromUtc, validToUtc);

    public bool IsValidAt(DateTime utcNow) => ValidFromUtc <= utcNow && (!ValidToUtc.HasValue || utcNow < ValidToUtc.Value);

    public void Update(CustomerTier tier, decimal discountPercentage, DateTime validFromUtc, DateTime? validToUtc)
        => SetValues(ProductId, tier, discountPercentage, validFromUtc, validToUtc);

    private void SetValues(int productId, CustomerTier tier, decimal discountPercentage, DateTime validFromUtc, DateTime? validToUtc)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        if (discountPercentage < 0 || discountPercentage > 100) throw new ArgumentOutOfRangeException(nameof(discountPercentage));
        if (validToUtc.HasValue && validToUtc.Value <= validFromUtc) throw new ArgumentException("Validity end must be after validity start.");
        ProductId = productId; Tier = tier; DiscountPercentage = discountPercentage; ValidFromUtc = validFromUtc; ValidToUtc = validToUtc;
    }
}
