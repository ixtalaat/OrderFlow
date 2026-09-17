namespace OrderFlow.Application.Inventory.DTOs;

public sealed record InventoryResponse(int ProductId, int Quantity, int ReservedQuantity, int AvailableQuantity, int Version);
