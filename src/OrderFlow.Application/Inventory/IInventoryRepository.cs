using DomainInventory = OrderFlow.Domain.Entities.Inventory;

namespace OrderFlow.Application.Inventory;

public interface IInventoryRepository
{
    Task<DomainInventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task AddAsync(DomainInventory inventory, CancellationToken cancellationToken = default);
}
