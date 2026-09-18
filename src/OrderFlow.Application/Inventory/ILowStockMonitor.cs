namespace OrderFlow.Application.Inventory;

public interface ILowStockMonitor
{
    Task CheckAsync(int productId, int availableBefore, int availableAfter, CancellationToken cancellationToken = default);
}
