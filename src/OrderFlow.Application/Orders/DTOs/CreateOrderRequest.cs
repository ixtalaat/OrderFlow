namespace OrderFlow.Application.Orders.DTOs;

public sealed record CreateOrderRequest(IReadOnlyList<CreateOrderItemRequest> Items);
