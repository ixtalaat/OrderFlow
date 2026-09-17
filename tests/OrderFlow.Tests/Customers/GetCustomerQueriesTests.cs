using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Customers.Queries.GetCustomerById;
using OrderFlow.Application.Customers.Queries.GetCustomerByUserId;
using OrderFlow.Application.Customers.Queries.GetCustomers;

namespace OrderFlow.Tests.Customers;

public class GetCustomerQueriesTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();

    [Fact]
    public async Task GetCustomerById_Should_Return_NotFound_When_Missing()
    {
        // Arrange
        _customerRepository.GetResponseByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((CustomerResponse?)null);

        var handler = new GetCustomerByIdQueryHandler(_customerRepository);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(999), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.NotFound);
    }

    [Fact]
    public async Task GetCustomerById_Should_Return_Customer_When_Found()
    {
        // Arrange
        var customer = new CustomerResponse(1, "user-1", "John", "john@test.com", "+123", "Addr", true, DateTime.UtcNow, null);
        _customerRepository.GetResponseByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(customer);

        var handler = new GetCustomerByIdQueryHandler(_customerRepository);

        // Act
        var result = await handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(customer);
    }

    [Fact]
    public async Task GetCustomerByUserId_Should_Return_Customer_When_Found()
    {
        // Arrange
        var customer = new CustomerResponse(1, "user-1", "John", "john@test.com", "+123", "Addr", true, DateTime.UtcNow, null);
        _customerRepository.GetResponseByUserIdAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(customer);

        var handler = new GetCustomerByUserIdQueryHandler(_customerRepository);

        // Act
        var result = await handler.Handle(new GetCustomerByUserIdQuery("user-1"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(customer);
    }

    [Fact]
    public async Task GetCustomers_Should_Return_PagedList()
    {
        // Arrange
        var queryParams = new CustomerQueryParams("John", true, 1, 10);
        var items = new List<CustomerResponse>
        {
            new(1, "user-1", "John", "john@test.com", "+123", "Addr", true, DateTime.UtcNow, null)
        };
        var pagedList = new PagedList<CustomerResponse>(items, 1, 1, 10);

        _customerRepository.GetPagedListAsync(queryParams, Arg.Any<CancellationToken>())
            .Returns(pagedList);

        var handler = new GetCustomersQueryHandler(_customerRepository);

        // Act
        var result = await handler.Handle(new GetCustomersQuery(queryParams), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pagedList);
        result.Value!.Items.Should().HaveCount(1);
    }
}
