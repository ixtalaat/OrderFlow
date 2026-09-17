namespace OrderFlow.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem() { }

    private OrderItem(int productId, int quantity, decimal unitPrice)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        ProductId = productId; Quantity = quantity; UnitPrice = unitPrice;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;

    public static OrderItem Create(int productId, int quantity, decimal unitPrice) => new(productId, quantity, unitPrice);
    public void IncreaseQuantity(int quantity) { if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity)); Quantity = checked(Quantity + quantity); }
}
