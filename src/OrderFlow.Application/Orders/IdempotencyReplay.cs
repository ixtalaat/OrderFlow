using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders;

public sealed record IdempotencyReplay(bool PayloadMatches, OrderResponse? Order);
