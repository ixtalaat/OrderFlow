using FluentAssertions;
using NSubstitute;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Coupons;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Products;
using OrderFlow.Domain.Entities;
using DomainInventory = OrderFlow.Domain.Entities.Inventory;

namespace OrderFlow.Tests.Orders;

public sealed class OrderCouponTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IInventoryRepository _inventories = Substitute.For<IInventoryRepository>();
    private readonly IPricingService _pricing = Substitute.For<IPricingService>();
    private readonly ICouponRepository _coupons = Substitute.For<ICouponRepository>();
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IBackgroundJobScheduler _backgroundJobs = Substitute.For<IBackgroundJobScheduler>();

    private readonly Product _product;
    private readonly DomainInventory _inventory;

    public OrderCouponTests()
    {
        _product = Product.Create("Product", "Description", "SKU-1", 100m, 1);
        typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(_product, 7);
        _inventory = DomainInventory.Create(7);
        _inventory.AddStock(10);

        _customers.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Customer.Create("user-1", "+123", "Address"));
        _products.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(_product);
        _inventories.GetByProductIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(_inventory);
        _pricing.GetPricesAsync(Arg.Any<IReadOnlyDictionary<int, decimal>>(), Arg.Any<CustomerTier>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, decimal> { [7] = 100m });
        _orders.GetResponseByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((OrderResponse?)null);
    }

    private CreateOrderCommandHandler Handler => new(
        _customers, _products, _inventories, _pricing, _coupons, _orders,
        _unitOfWork, _backgroundJobs, Substitute.For<ILogger<CreateOrderCommandHandler>>());

    [Fact]
    public async Task Should_Apply_Coupon_Discount_To_Order_Total()
    {
        var coupon = Coupon.Create("SAVE10", 10, 0, DateTime.UtcNow.AddDays(-1), null, null);
        _coupons.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(coupon);

        Order? saved = null;
        await _orders.AddAsync(Arg.Do<Order>(x => saved = x), Arg.Any<CancellationToken>());
        var result = await Handler.Handle(
            new CreateOrderCommand(1, new[] { new CreateOrderItemRequest(7, 1) }, "save10"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        saved!.CouponCode.Should().Be("SAVE10");
        saved.DiscountAmount.Should().Be(10m);
        saved.TotalAmount.Should().Be(90m);
        coupon.TimesRedeemed.Should().Be(1);
    }

    [Fact]
    public async Task Should_Reject_Unknown_Coupon()
    {
        _coupons.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Coupon?)null);

        var result = await Handler.Handle(
            new CreateOrderCommand(1, new[] { new CreateOrderItemRequest(7, 1) }, "NOPE"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidCoupon);
    }

    [Fact]
    public async Task Should_Reject_Exhausted_Coupon()
    {
        var coupon = Coupon.Create("ONEUSE", 10, 0, DateTime.UtcNow.AddDays(-1), null, 1);
        coupon.Redeem();
        _coupons.GetByCodeAsync("ONEUSE", Arg.Any<CancellationToken>()).Returns(coupon);

        var result = await Handler.Handle(
            new CreateOrderCommand(1, new[] { new CreateOrderItemRequest(7, 1) }, "ONEUSE"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidCoupon);
    }
}
