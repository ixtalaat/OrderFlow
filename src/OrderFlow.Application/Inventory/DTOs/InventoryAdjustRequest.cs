namespace OrderFlow.Application.Inventory.DTOs;

public sealed record InventoryAdjustRequest(int Quantity, string Reason);
