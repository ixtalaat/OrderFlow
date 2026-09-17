using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Inventory;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Products;

public sealed class InventoryRepository(ApplicationDbContext db) : IInventoryRepository
{
    public Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        => db.Inventories.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

    public Task AddAsync(Inventory inventory, CancellationToken cancellationToken = default)
        => db.Inventories.AddAsync(inventory, cancellationToken).AsTask();
}
