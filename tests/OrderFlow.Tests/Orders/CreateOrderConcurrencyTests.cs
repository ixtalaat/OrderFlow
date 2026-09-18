using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Coupons;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Inventory.Commands.ReserveStock;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Products;
using OrderFlow.Domain.Entities;
using InventoryEntity = OrderFlow.Domain.Entities.Inventory;

namespace OrderFlow.Tests.Orders;

public sealed class CreateOrderConcurrencyTests
{
    [Fact]
    public async Task CreateOrder_Should_Return_Conflict_When_Reservation_Collides()
    {
        var customers = Substitute.For<ICustomerRepository>();
        var products = Substitute.For<IProductRepository>();
        var inventories = Substitute.For<IInventoryRepository>();
        var pricing = Substitute.For<IPricingService>();
        var orders = Substitute.For<IOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var backgroundJobs = Substitute.For<IBackgroundJobScheduler>();

        var customer = Customer.Create("user-1", "+123", "Address");
        var product = Product.Create("Product", "Description", "SKU-1", 10m, 1);
        typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(product, 7);
        var inventory = InventoryEntity.Create(7);
        inventory.AddStock(5);

        customers.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(customer);
        products.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(product);
        inventories.GetByProductIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(inventory);
        pricing.GetPricesAsync(Arg.Any<IReadOnlyDictionary<int, decimal>>(), Arg.Any<CustomerTier>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, decimal> { [7] = 10m });
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Throws(new ConcurrencyConflictException("The data was changed by another request."));

        var handler = new CreateOrderCommandHandler(customers, products, inventories, pricing, Substitute.For<ICouponRepository>(), orders, unitOfWork, backgroundJobs, Substitute.For<ILogger<CreateOrderCommandHandler>>());
        var result = await handler.Handle(
            new CreateOrderCommand(1, new[] { new CreateOrderItemRequest(7, 5) }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.ConcurrencyConflict);
    }

    [Fact]
    public async Task ReserveStock_Should_Return_Conflict_When_Save_Collides()
    {
        var inventories = Substitute.For<IInventoryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventories.GetByProductIdAsync(1, Arg.Any<CancellationToken>()).Returns(inventory);
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Throws(new ConcurrencyConflictException("The data was changed by another request."));

        var handler = new ReserveStockCommandHandler(inventories, unitOfWork, Substitute.For<ILowStockMonitor>(), Substitute.For<ILogger<ReserveStockCommandHandler>>());
        var result = await handler.Handle(
            new ReserveStockCommand(1, 5),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderFlow.Application.Inventory.InventoryErrors.ConcurrencyConflict);
    }
}
