using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Coupons;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(ICustomerRepository customers, IProductRepository products, IInventoryRepository inventories, IPricingService pricing, ICouponRepository coupons, IOrderRepository orders, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs, ILogger<CreateOrderCommandHandler> logger) : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, ct);
        if (customer is null || !customer.IsActive) return Result.Failure<OrderResponse>(OrderErrors.CustomerNotFound);
        var order = Order.Create(command.CustomerId);
        var lines = new List<(Product Product, int Quantity)>();
        foreach (var group in command.Items.GroupBy(x => x.ProductId))
        {
            var quantity = group.Sum(x => x.Quantity);
            var product = await products.GetByIdAsync(group.Key, ct);
            if (product is null || !product.IsActive) return Result.Failure<OrderResponse>(OrderErrors.ProductUnavailable);
            var inventory = await inventories.GetByProductIdAsync(product.Id, ct);
            if (inventory is null || quantity > inventory.AvailableQuantity) return Result.Failure<OrderResponse>(OrderErrors.InsufficientStock);
            inventory.ReserveStock(quantity);
            lines.Add((product, quantity));
        }

        var prices = await pricing.GetPricesAsync(lines.ToDictionary(x => x.Product.Id, x => x.Product.Price), customer.Tier, ct);
        foreach (var line in lines) order.AddItem(OrderItem.Create(line.Product.Id, line.Quantity, prices[line.Product.Id]));
        if (command.CouponCode is not null)
        {
            var coupon = await coupons.GetByCodeAsync(command.CouponCode, ct);
            var subtotal = order.TotalAmount;
            if (coupon is null || !coupon.CanRedeem(subtotal, DateTime.UtcNow))
                return Result.Failure<OrderResponse>(OrderErrors.InvalidCoupon);
            order.ApplyCoupon(coupon.Code, subtotal * coupon.DiscountPercentage / 100);
            coupon.Redeem();
        }
        order.Submit();
        await orders.AddAsync(order, ct);
        backgroundJobs.EnqueueOrderNotification(order);
        try
        {
            // Single unit-of-work save persists the order, inventory
            // reservations, and outbox row atomically. The Inventory.Version
            // concurrency token makes overlapping reservations conflict here
            // instead of overselling.
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogWarning("Order creation conflicted for customer {CustomerId}; inventory changed concurrently.", command.CustomerId);
            return Result.Failure<OrderResponse>(OrderErrors.ConcurrencyConflict);
        }
        logger.LogInformation("Order {OrderId} created for customer {CustomerId} with {ItemCount} items.", order.Id, command.CustomerId, lines.Count);
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
