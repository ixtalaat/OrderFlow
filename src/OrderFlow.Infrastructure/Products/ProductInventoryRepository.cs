using OrderFlow.Application.Products;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Products;

public sealed class ProductInventoryRepository(ApplicationDbContext db) : IProductInventoryRepository
{
    public Task AddAsync(ProductInventory inventory, CancellationToken cancellationToken = default)
        => db.ProductInventories.AddAsync(inventory, cancellationToken).AsTask();
}
