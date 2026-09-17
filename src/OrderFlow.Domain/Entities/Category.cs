namespace OrderFlow.Domain.Entities;

public sealed class Category
{
    private Category() { }

    private Category(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public static Category Create(string name) => new(name);
}
