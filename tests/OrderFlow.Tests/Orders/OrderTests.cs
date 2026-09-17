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
}
