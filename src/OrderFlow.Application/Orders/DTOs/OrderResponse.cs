using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.DTOs;

public sealed record OrderResponse(int Id, int CustomerId, OrderStatus Status, decimal TotalAmount, DateTime CreatedAtUtc, IReadOnlyList<OrderItemResponse> Items, AccountingSyncStatus AccountingSyncStatus, string? ExternalInvoiceId, int AccountingSyncAttempts, DateTime? AccountingLastAttemptAtUtc, string? AccountingLastError, string? CouponCode = null, decimal DiscountAmount = 0);
