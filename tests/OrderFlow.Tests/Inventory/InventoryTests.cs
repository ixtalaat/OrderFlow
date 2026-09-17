using FluentAssertions;
using InventoryEntity = OrderFlow.Domain.Entities.Inventory;

namespace OrderFlow.Tests.Inventory;

public sealed class InventoryTests
{
    [Fact]
    public void Add_Reserve_Release_And_Confirm_Should_Apply_Stock_Rules()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(10);
        inventory.ReserveStock(4);
        inventory.AvailableQuantity.Should().Be(6);
        inventory.ReleaseStock(1);
        inventory.ConfirmStock(2);
        inventory.Quantity.Should().Be(8);
        inventory.ReservedQuantity.Should().Be(1);
        inventory.AvailableQuantity.Should().Be(7);
        inventory.Version.Should().Be(4);
    }

    [Fact]
    public void Reserve_Should_Reject_Insufficient_Stock()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(2);
        var action = () => inventory.ReserveStock(3);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adjust_Should_Not_Go_Below_Reserved_Quantity()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventory.ReserveStock(3);
        var action = () => inventory.AdjustStock(-3);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Operations_Should_Reject_Non_Positive_Quantities()
    {
        var inventory = InventoryEntity.Create(1);
        FluentActions.Invoking(() => inventory.AddStock(0)).Should().Throw<ArgumentOutOfRangeException>();
        FluentActions.Invoking(() => inventory.ReserveStock(-1)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Reserve_Should_Accept_Exact_Available_Quantity()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventory.ReserveStock(5);
        inventory.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void Release_Should_Reject_More_Than_Reserved()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventory.ReserveStock(2);
        FluentActions.Invoking(() => inventory.ReleaseStock(3)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirm_Should_Reject_More_Than_Reserved()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventory.ReserveStock(2);
        FluentActions.Invoking(() => inventory.ConfirmStock(3)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adjust_Should_Accept_Quantity_Equal_To_Reserved()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        inventory.ReserveStock(3);
        inventory.AdjustStock(-2);
        inventory.Quantity.Should().Be(3);
    }

    [Fact]
    public void Adjust_Should_Reject_Zero_Quantity()
    {
        var inventory = InventoryEntity.Create(1);
        inventory.AddStock(5);
        FluentActions.Invoking(() => inventory.AdjustStock(0)).Should().Throw<ArgumentOutOfRangeException>();
    }
}
