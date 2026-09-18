using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler(IInventoryRepository inventories, IUnitOfWork unitOfWork, ILowStockMonitor lowStock, ILogger<ReserveStockCommandHandler> logger) : IRequestHandler<ReserveStockCommand, Result<InventoryResponse>>
{
    public async Task<Result<InventoryResponse>> Handle(ReserveStockCommand command, CancellationToken ct)
    {
        if (command.Quantity <= 0) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidQuantity);
        var inventory = await inventories.GetByProductIdAsync(command.ProductId, ct);
        if (inventory is null) return Result.Failure<InventoryResponse>(InventoryErrors.NotFound);
        if (command.Quantity > inventory.AvailableQuantity) return Result.Failure<InventoryResponse>(InventoryErrors.InsufficientStock);
        var availableBefore = inventory.AvailableQuantity;
        inventory.ReserveStock(command.Quantity);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<InventoryResponse>(InventoryErrors.ConcurrencyConflict);
        }
        logger.LogInformation("Reserved {Quantity} units for product {ProductId}; {AvailableQuantity} available.", command.Quantity, command.ProductId, inventory.AvailableQuantity);
        await lowStock.CheckAsync(command.ProductId, availableBefore, inventory.AvailableQuantity, ct);
        return Result.Success(new InventoryResponse(inventory.ProductId, inventory.Quantity, inventory.ReservedQuantity, inventory.AvailableQuantity, inventory.Version));
    }
}
