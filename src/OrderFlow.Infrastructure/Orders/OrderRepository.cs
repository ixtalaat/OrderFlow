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

    public Task AddAsync(Order order, CancellationToken ct = default) => db.Orders.AddAsync(order, ct).AsTask();

    private static OrderResponse Map(Order order) => new(order.Id, order.CustomerId, order.Status, order.TotalAmount, order.CreatedAtUtc, order.Items.Select(x => new OrderItemResponse(x.ProductId, x.Product.Name, x.Quantity, x.UnitPrice, x.LineTotal)).ToList());
}
