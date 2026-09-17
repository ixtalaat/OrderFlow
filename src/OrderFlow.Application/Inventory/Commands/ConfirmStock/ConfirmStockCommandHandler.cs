using MediatR;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Commands.ConfirmStock;

public sealed class ConfirmStockCommandHandler(IInventoryRepository inventories, IUnitOfWork unitOfWork) : IRequestHandler<ConfirmStockCommand, Result<InventoryResponse>>
{
    public async Task<Result<InventoryResponse>> Handle(ConfirmStockCommand command, CancellationToken ct)
    {
        if (command.Quantity <= 0) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidQuantity);
        var inventory = await inventories.GetByProductIdAsync(command.ProductId, ct);
        if (inventory is null) return Result.Failure<InventoryResponse>(InventoryErrors.NotFound);
        if (command.Quantity > inventory.ReservedQuantity) return Result.Failure<InventoryResponse>(InventoryErrors.InvalidReservation);
        inventory.ConfirmStock(command.Quantity);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<InventoryResponse>(InventoryErrors.ConcurrencyConflict);
        }
        return Result.Success(new InventoryResponse(inventory.ProductId, inventory.Quantity, inventory.ReservedQuantity, inventory.AvailableQuantity, inventory.Version));
    }
}
