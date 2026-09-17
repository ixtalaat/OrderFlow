namespace OrderFlow.Domain.Entities;

public sealed class Inventory
{
    private Inventory() { }

    private Inventory(int productId)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        ProductId = productId;
    }

    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public int Version { get; private set; }

    public static Inventory Create(int productId) => new(productId);

    public void AddStock(int quantity)
    {
        EnsurePositive(quantity);
        Quantity = checked(Quantity + quantity);
        Version++;
    }

    public void ReserveStock(int quantity)
    {
        EnsurePositive(quantity);
        if (quantity > AvailableQuantity) throw new InvalidOperationException("Insufficient available stock.");
        ReservedQuantity += quantity;
        Version++;
    }

    public void ReleaseStock(int quantity)
    {
        EnsurePositive(quantity);
        if (quantity > ReservedQuantity) throw new InvalidOperationException("Cannot release more than reserved stock.");
        ReservedQuantity -= quantity;
        Version++;
    }

    public void ConfirmStock(int quantity)
    {
        EnsurePositive(quantity);
        if (quantity > ReservedQuantity) throw new InvalidOperationException("Cannot confirm more than reserved stock.");
        Quantity -= quantity;
        ReservedQuantity -= quantity;
        Version++;
    }

    public void AdjustStock(int quantity)
    {
        if (quantity == 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        var newQuantity = checked(Quantity + quantity);
        if (newQuantity < ReservedQuantity) throw new InvalidOperationException("Stock cannot be adjusted below reserved stock.");
        Quantity = newQuantity;
        Version++;
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
    }
}
