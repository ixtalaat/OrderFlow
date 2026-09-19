using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.DTOs;

public sealed record OrderStatusSummary(OrderStatus Status, int Count, decimal Revenue);
