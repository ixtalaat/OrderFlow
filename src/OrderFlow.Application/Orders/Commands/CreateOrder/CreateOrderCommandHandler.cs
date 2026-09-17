using MediatR;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products;
using OrderFlow.Application.Pricing;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(ICustomerRepository customers, IProductRepository products, IInventoryRepository inventories, IPricingService pricing, IOrderRepository orders, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs) : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, ct);
        if (customer is null || !customer.IsActive) return Result.Failure<OrderResponse>(OrderErrors.CustomerNotFound);
        var order = Order.Create(command.CustomerId);
        foreach (var group in command.Items.GroupBy(x => x.ProductId))
        {
            var quantity = group.Sum(x => x.Quantity);
            var product = await products.GetByIdAsync(group.Key, ct);
            if (product is null || !product.IsActive) return Result.Failure<OrderResponse>(OrderErrors.ProductUnavailable);
            var inventory = await inventories.GetByProductIdAsync(product.Id, ct);
            if (inventory is null || quantity > inventory.AvailableQuantity) return Result.Failure<OrderResponse>(OrderErrors.InsufficientStock);
            inventory.ReserveStock(quantity);
            var price = await pricing.GetPriceAsync(product.Id, product.Price, customer.Tier, ct);
            order.AddItem(OrderItem.Create(product.Id, quantity, price));
        }
        order.Submit();
        await orders.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);
        backgroundJobs.EnqueueOrderNotification(order.Id, order.Status);
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
