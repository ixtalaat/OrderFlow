using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory.DTOs;

namespace OrderFlow.Application.Inventory.Queries.GetInventory;

public sealed class GetInventoryQueryHandler(IInventoryRepository inventories) : IRequestHandler<GetInventoryQuery, Result<InventoryResponse>>
{
    public async Task<Result<InventoryResponse>> Handle(GetInventoryQuery query, CancellationToken ct)
    {
        var inventory = await inventories.GetByProductIdAsync(query.ProductId, ct);
        return inventory is null ? Result.Failure<InventoryResponse>(InventoryErrors.NotFound) : Result.Success(new InventoryResponse(inventory.ProductId, inventory.Quantity, inventory.ReservedQuantity, inventory.AvailableQuantity, inventory.Version));
    }
}
