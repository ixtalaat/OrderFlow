namespace OrderFlow.Domain.Entities;

public sealed class Product
{
    private Product() { }

    private Product(string name, string description, string sku, decimal price, int categoryId)
    {
        SetDetails(name, description, sku, price, categoryId);
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public static Product Create(string name, string description, string sku, decimal price, int categoryId)
        => new(name, description, sku, price, categoryId);

    public void UpdateDetails(string name, string description, string sku, decimal price, int categoryId)
    {
        SetDetails(name, description, sku, price, categoryId);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (!IsActive) { IsActive = true; UpdatedAtUtc = DateTime.UtcNow; }
    }

    public void Deactivate()
    {
        if (IsActive) { IsActive = false; UpdatedAtUtc = DateTime.UtcNow; }
    }

    private void SetDetails(string name, string description, string sku, decimal price, int categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
        ArgumentException.ThrowIfNullOrWhiteSpace(sku, nameof(sku));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (categoryId <= 0) throw new ArgumentOutOfRangeException(nameof(categoryId));
        Name = name.Trim(); Description = description.Trim(); Sku = sku.Trim().ToUpperInvariant();
        Price = price; CategoryId = categoryId;
    }
}
