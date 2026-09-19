using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Orders;

public sealed class OrderRepository(ApplicationDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<OrderResponse?> GetResponseByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await db.Orders.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.Id == id, ct);
        return order is null ? null : Map(order);
    }

    public async Task<PagedList<OrderResponse>> GetCustomerOrdersAsync(int customerId, OrderQueryParams parameters, CancellationToken ct = default)
    {
        var query = db.Orders.AsNoTracking().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.CreatedAtUtc);
        var total = await query.CountAsync(ct);
        var orders = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Include(x => x.Items).ThenInclude(x => x.Product).ToListAsync(ct);
        return new PagedList<OrderResponse>(orders.Select(Map).ToList(), total, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedList<OrderResponse>> GetAllOrdersAsync(OrderQueryParams parameters, CancellationToken ct = default)
    {
        var query = db.Orders.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc);
        var total = await query.CountAsync(ct);
        var orders = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Include(x => x.Items).ThenInclude(x => x.Product).ToListAsync(ct);
        return new PagedList<OrderResponse>(orders.Select(Map).ToList(), total, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<IReadOnlyList<OrderStatusSummary>> GetSalesSummaryAsync(CancellationToken ct = default)
    {
        var orders = await db.Orders.AsNoTracking()
            .Select(x => new { x.Id, x.Status, x.DiscountAmount })
            .ToListAsync(ct);
        var lines = await db.Set<OrderItem>().AsNoTracking()
            .Select(i => new { i.OrderId, i.LineTotal })
            .ToListAsync(ct);
        var totalsByOrder = lines
            .GroupBy(l => l.OrderId)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.LineTotal));
        return orders
            .GroupBy(x => x.Status)
            .Select(g => new OrderStatusSummary(
                g.Key,
                g.Count(),
                g.Sum(x => (totalsByOrder.TryGetValue(x.Id, out var total) ? total : 0) - x.DiscountAmount)))
            .ToList();
    }

    public Task AddAsync(Order order, CancellationToken ct = default) => db.Orders.AddAsync(order, ct).AsTask();

    private static OrderResponse Map(Order order) => new(order.Id, order.CustomerId, order.Status, order.TotalAmount, order.CreatedAtUtc, order.Items.Select(x => new OrderItemResponse(x.ProductId, x.Product.Name, x.Quantity, x.UnitPrice, x.LineTotal)).ToList(), order.AccountingSyncStatus, order.ExternalInvoiceId, order.AccountingSyncAttempts, order.AccountingLastAttemptAtUtc, order.AccountingLastError, order.CouponCode, order.DiscountAmount);
}
