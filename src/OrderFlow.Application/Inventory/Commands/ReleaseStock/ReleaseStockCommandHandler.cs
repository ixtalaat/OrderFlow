using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ReleaseStock;

public sealed class ReleaseStockCommandHandler(IInventoryRepository inventories, IUnitOfWork unitOfWork, ILogger<ReleaseStockCommandHandler> logger) : IRequestHandler<ReleaseStockCommand, Result<InventoryResponse>>
{
    public async Task<Result<InventoryResponse>> Handle(ReleaseStockCommand command, CancellationToken ct)
    {
        if (command.Quantity <= 0) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidQuantity);
        var inventory = await inventories.GetByProductIdAsync(command.ProductId, ct);
        if (inventory is null) return Result.Failure<InventoryResponse>(InventoryErrors.NotFound);
        if (command.Quantity > inventory.ReservedQuantity) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidReservation);
        inventory.ReleaseStock(command.Quantity);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<InventoryResponse>(InventoryErrors.ConcurrencyConflict);
        }
        logger.LogInformation("Released {Quantity} units for product {ProductId}; {AvailableQuantity} available.", command.Quantity, command.ProductId, inventory.AvailableQuantity);
        return Result.Success(new InventoryResponse(inventory.ProductId, inventory.Quantity, inventory.ReservedQuantity, inventory.AvailableQuantity, inventory.Version));
    }
}
