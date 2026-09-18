using Microsoft.Extensions.Logging;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Products;

namespace OrderFlow.Application.Inventory;

public sealed class LowStockMonitor(
    IProductRepository products,
    IStaffNotificationService staff,
    ILogger<LowStockMonitor> logger) : ILowStockMonitor
{
    public async Task CheckAsync(int productId, int availableBefore, int availableAfter, CancellationToken cancellationToken = default)
    {
        if (availableAfter >= availableBefore)
            return;

        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product?.LowStockThreshold is not int threshold)
            return;

        if (availableBefore < threshold || availableAfter >= threshold)
            return;

        try
        {
            await staff.NotifyLowStockAsync(productId, availableAfter, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Low stock notification for product {ProductId} failed.", productId);
        }
    }
}
