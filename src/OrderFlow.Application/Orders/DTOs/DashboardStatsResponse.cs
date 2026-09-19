namespace OrderFlow.Application.Orders.DTOs;

public sealed record DashboardStatsResponse(
    int TotalOrders,
    int SubmittedOrders,
    int CompletedOrders,
    decimal TotalRevenue,
    int TotalProducts,
    int TotalCustomers,
    int LowStockProducts);
