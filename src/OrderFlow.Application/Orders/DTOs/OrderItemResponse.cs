namespace OrderFlow.Application.Orders.DTOs;

public sealed record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
