using FluentAssertions;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries.GetCustomerOrders;

namespace OrderFlow.Tests.Orders;

public sealed class OrderValidatorTests
{
    private readonly CreateOrderCommandValidator _createValidator = new();
    private readonly GetCustomerOrdersQueryValidator _queryValidator = new();

    [Fact]
    public void CreateOrderCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new CreateOrderCommand(1, new[] { new CreateOrderItemRequest(7, 5) });
        _createValidator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateOrderCommandValidator_Should_Fail_For_Empty_Items()
    {
        var command = new CreateOrderCommand(1, Array.Empty<CreateOrderItemRequest>());
        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 7, 5)]
    [InlineData(1, 0, 5)]
    [InlineData(1, 7, 0)]
    [InlineData(1, 7, -2)]
    public void CreateOrderCommandValidator_Should_Fail_For_Invalid_Ids_And_Quantities(
        int customerId, int productId, int quantity)
    {
        var command = new CreateOrderCommand(customerId, new[] { new CreateOrderItemRequest(productId, quantity) });
        _createValidator.Validate(command).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 1, 20, true)]
    [InlineData(0, 1, 20, false)]
    [InlineData(1, 0, 20, false)]
    [InlineData(1, 1, 101, false)]
    public void GetCustomerOrdersQueryValidator_Should_Validate_Customer_And_Pagination(
        int customerId, int pageNumber, int pageSize, bool expected)
    {
        var query = new GetCustomerOrdersQuery(customerId, new OrderQueryParams(pageNumber, pageSize));
        _queryValidator.Validate(query).IsValid.Should().Be(expected);
    }
}
