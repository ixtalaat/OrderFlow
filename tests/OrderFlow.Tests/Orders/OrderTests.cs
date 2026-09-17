using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Order_Should_Calculate_Total_And_Follow_Workflow()
    {
        var order = Order.Create(1);
        order.AddItem(OrderItem.Create(2, 2, 10));
        order.Submit();
        order.Confirm();
        order.Process();
        order.Complete();
        order.Status.Should().Be(OrderStatus.Completed);
        order.TotalAmount.Should().Be(20);
    }

    [Fact]
    public void Order_Should_Reject_Invalid_Transition()
    {
        var order = Order.Create(1);
        FluentActions.Invoking(order.Complete).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void OrderItem_Should_Reject_Invalid_Quantity()
    {
        FluentActions.Invoking(() => OrderItem.Create(1, 0, 10)).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Order_Should_Reject_Double_Submit()
    {
        var order = Order.Create(1);
        order.Submit();
        FluentActions.Invoking(order.Submit).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_Should_Reject_Confirm_From_Draft()
    {
        var order = Order.Create(1);
        FluentActions.Invoking(order.Confirm).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_Should_Reject_Reject_After_Confirm()
    {
        var order = Order.Create(1);
        order.Submit();
        order.Confirm();
        FluentActions.Invoking(order.Reject).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_Should_Reject_Process_Before_Confirm()
    {
        var order = Order.Create(1);
        order.Submit();
        FluentActions.Invoking(order.Process).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_Should_Reject_Complete_Before_Processing()
    {
        var order = Order.Create(1);
        order.Submit();
        order.Confirm();
        FluentActions.Invoking(order.Complete).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Order_Should_Reject_Null_Item()
    {
        var order = Order.Create(1);
        FluentActions.Invoking(() => order.AddItem(null!)).Should().Throw<ArgumentNullException>();
    }
}
