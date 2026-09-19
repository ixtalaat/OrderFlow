using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetResponseByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedList<OrderResponse>> GetCustomerOrdersAsync(int customerId, OrderQueryParams query, CancellationToken cancellationToken = default);
    Task<PagedList<OrderResponse>> GetAllOrdersAsync(OrderQueryParams query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderStatusSummary>> GetSalesSummaryAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
