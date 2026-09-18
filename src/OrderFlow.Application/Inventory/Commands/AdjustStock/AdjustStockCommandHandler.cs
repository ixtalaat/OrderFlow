using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler(IInventoryRepository inventories, IUnitOfWork unitOfWork, IAuditService audit, ILowStockMonitor lowStock, ILogger<AdjustStockCommandHandler> logger) : IRequestHandler<AdjustStockCommand, Result<InventoryResponse>>
{
    public async Task<Result<InventoryResponse>> Handle(AdjustStockCommand command, CancellationToken ct)
    {
        if (command.Quantity == 0) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidQuantity);
        if (string.IsNullOrWhiteSpace(command.Reason)) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidReason);
        var inventory = await inventories.GetByProductIdAsync(command.ProductId, ct);
        if (inventory is null) return Result.Failure<InventoryResponse>(InventoryErrors.NotFound);
        if (command.Quantity < 0 && inventory.Quantity + command.Quantity < inventory.ReservedQuantity) return Result.Failure<InventoryResponse>(InventoryErrors.InsufficientStock);
        var availableBefore = inventory.AvailableQuantity;
        inventory.AdjustStock(command.Quantity);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<InventoryResponse>(InventoryErrors.ConcurrencyConflict);
        }
        logger.LogInformation("Adjusted stock by {Quantity} units for product {ProductId}; {AvailableQuantity} available.", command.Quantity, command.ProductId, inventory.AvailableQuantity);
        await audit.LogAsync("StockAdjusted", "Inventory", command.ProductId.ToString(), $"Quantity {command.Quantity}: {command.Reason}", ct);
        await audit.TrySaveAsync(ct);
        await lowStock.CheckAsync(command.ProductId, availableBefore, inventory.AvailableQuantity, ct);
        return Result.Success(new InventoryResponse(inventory.ProductId, inventory.Quantity, inventory.ReservedQuantity, inventory.AvailableQuantity, inventory.Version));
    }
}
