using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Inventory;

public static class InventoryErrors
{
    public static readonly Error NotFound = new("Inventory.NotFound", "Inventory not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidQuantity = new("Inventory.InvalidQuantity", "Quantity must be greater than zero.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidReason = new("Inventory.InvalidReason", "A stocktake reason is required.", StatusCodes.Status400BadRequest);
    public static readonly Error InsufficientStock = new("Inventory.InsufficientStock", "Insufficient available stock.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidReservation = new("Inventory.InvalidReservation", "The requested reservation is invalid.", StatusCodes.Status409Conflict);
    public static readonly Error ConcurrencyConflict = new("Inventory.ConcurrencyConflict", "Inventory was changed by another request.", StatusCodes.Status409Conflict);
}
