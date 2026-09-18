namespace OrderFlow.Domain.Entities;

public sealed class Coupon
{
    private Coupon() { }

    private Coupon(string code, decimal discountPercentage, decimal minOrderTotal, DateTime validFromUtc, DateTime? validToUtc, int? maxRedemptions)
    {
        SetValues(code, discountPercentage, minOrderTotal, validFromUtc, validToUtc, maxRedemptions);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public decimal DiscountPercentage { get; private set; }
    public decimal MinOrderTotal { get; private set; }
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }
    public int? MaxRedemptions { get; private set; }
    public int TimesRedeemed { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Coupon Create(string code, decimal discountPercentage, decimal minOrderTotal, DateTime validFromUtc, DateTime? validToUtc, int? maxRedemptions)
        => new(code, discountPercentage, minOrderTotal, validFromUtc, validToUtc, maxRedemptions);

    public void Update(decimal discountPercentage, decimal minOrderTotal, DateTime validFromUtc, DateTime? validToUtc, int? maxRedemptions)
    {
        SetValues(Code, discountPercentage, minOrderTotal, validFromUtc, validToUtc, maxRedemptions);
        Version++;
        Touch();
    }

    public void Activate()
    {
        if (!IsActive) { IsActive = true; Version++; Touch(); }
    }

    public void Deactivate()
    {
        if (IsActive) { IsActive = false; Version++; Touch(); }
    }

    public bool IsValidAt(DateTime utcNow)
        => IsActive && ValidFromUtc <= utcNow && (!ValidToUtc.HasValue || utcNow < ValidToUtc.Value);

    public bool CanRedeem(decimal orderTotal, DateTime utcNow)
        => IsValidAt(utcNow) && orderTotal >= MinOrderTotal && (!MaxRedemptions.HasValue || TimesRedeemed < MaxRedemptions.Value);

    public void Redeem()
    {
        if (MaxRedemptions.HasValue && TimesRedeemed >= MaxRedemptions.Value)
            throw new InvalidOperationException("Coupon redemption limit reached.");
        TimesRedeemed++;
        Version++;
        Touch();
    }

    private void SetValues(string code, decimal discountPercentage, decimal minOrderTotal, DateTime validFromUtc, DateTime? validToUtc, int? maxRedemptions)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (discountPercentage <= 0 || discountPercentage > 100) throw new ArgumentOutOfRangeException(nameof(discountPercentage));
        if (minOrderTotal < 0) throw new ArgumentOutOfRangeException(nameof(minOrderTotal));
        if (validToUtc.HasValue && validToUtc.Value <= validFromUtc) throw new ArgumentException("Validity end must be after validity start.");
        if (maxRedemptions.HasValue && maxRedemptions.Value < 0) throw new ArgumentOutOfRangeException(nameof(maxRedemptions));
        Code = code.Trim().ToUpperInvariant();
        DiscountPercentage = discountPercentage;
        MinOrderTotal = minOrderTotal;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        MaxRedemptions = maxRedemptions;
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
