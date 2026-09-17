using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Products;

public interface IProductInventoryRepository
{
    Task AddAsync(ProductInventory inventory, CancellationToken cancellationToken = default);
}
