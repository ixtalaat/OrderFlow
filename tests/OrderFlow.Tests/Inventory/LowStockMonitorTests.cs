using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Products;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Inventory;

public sealed class LowStockMonitorTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IStaffNotificationService _staff = Substitute.For<IStaffNotificationService>();
    private readonly LowStockMonitor _monitor;

    public LowStockMonitorTests()
    {
        _monitor = new LowStockMonitor(
            _products, _staff, Substitute.For<ILogger<LowStockMonitor>>());
    }

    [Fact]
    public async Task Should_Notify_When_Crossing_Threshold_Downward()
    {
        var product = Product.Create("Product", "Description", "SKU-1", 10m, 1);
        product.SetLowStockThreshold(3);
        _products.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(product);

        await _monitor.CheckAsync(7, 4, 2);

        await _staff.Received(1).NotifyLowStockAsync(7, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Notify_When_Already_Below_Threshold()
    {
        var product = Product.Create("Product", "Description", "SKU-1", 10m, 1);
        product.SetLowStockThreshold(5);
        _products.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(product);

        await _monitor.CheckAsync(7, 4, 2);

        await _staff.DidNotReceive().NotifyLowStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Notify_When_No_Threshold()
    {
        var product = Product.Create("Product", "Description", "SKU-1", 10m, 1);
        _products.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(product);

        await _monitor.CheckAsync(7, 10, 1);

        await _staff.DidNotReceive().NotifyLowStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Notify_When_Stock_Increases()
    {
        await _monitor.CheckAsync(7, 1, 10);

        await _staff.DidNotReceive().NotifyLowStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _products.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Swallow_Notification_Failures()
    {
        var product = Product.Create("Product", "Description", "SKU-1", 10m, 1);
        product.SetLowStockThreshold(3);
        _products.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(product);
        _staff.NotifyLowStockAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("SMTP down.")));

        var action = () => _monitor.CheckAsync(7, 4, 2);

        await action.Should().NotThrowAsync();
    }
}
