namespace OrderFlow.Application.Orders.DTOs;

public sealed record CreateOrderItemRequest(int ProductId, int Quantity);
