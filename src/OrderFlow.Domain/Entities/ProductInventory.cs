namespace OrderFlow.Domain.Entities;

public sealed class ProductInventory
{
    private ProductInventory() { }

    private ProductInventory(int productId)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        ProductId = productId;
    }

    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int AvailableQuantity { get; private set; }

    public static ProductInventory Create(int productId) => new(productId);
}
