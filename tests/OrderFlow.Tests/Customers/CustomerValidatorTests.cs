using FluentAssertions;
using OrderFlow.Application.Customers.Commands.CreateCustomer;
using OrderFlow.Application.Customers.Commands.UpdateCustomer;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Customers.Queries.GetCustomers;

namespace OrderFlow.Tests.Customers;

public class CustomerValidatorTests
{
    private readonly CreateCustomerCommandValidator _createValidator = new();
    private readonly UpdateCustomerCommandValidator _updateValidator = new();
    private readonly GetCustomersQueryValidator _queryValidator = new();

    [Fact]
    public void CreateCustomerCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new CreateCustomerCommand("John Doe", "john@example.com", "Password@123", "+1234567890", "123 Main St");
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "john@example.com", "Password@123", "+1234567890", "123 Main St")]
    [InlineData("John Doe", "invalid-email", "Password@123", "+1234567890", "123 Main St")]
    [InlineData("John Doe", "john@example.com", "short", "+1234567890", "123 Main St")]
    [InlineData("John Doe", "john@example.com", "Password@123", "", "123 Main St")]
    [InlineData("John Doe", "john@example.com", "Password@123", "+1234567890", "")]
    public void CreateCustomerCommandValidator_Should_Fail_For_Invalid_Inputs(
        string name, string email, string password, string phone, string address)
    {
        var command = new CreateCustomerCommand(name, email, password, phone, address);
        var result = _createValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateCustomerCommandValidator_Should_Pass_For_Valid_Command()
    {
        var command = new UpdateCustomerCommand(1, "John Doe", "+1234567890", "123 Main St");
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, "John Doe", "+1234567890", "123 Main St")]
    [InlineData(-1, "John Doe", "+1234567890", "123 Main St")]
    [InlineData(1, "", "+1234567890", "123 Main St")]
    [InlineData(1, "John Doe", "", "123 Main St")]
    [InlineData(1, "John Doe", "+1234567890", "")]
    public void UpdateCustomerCommandValidator_Should_Fail_For_Invalid_Inputs(
        int id, string name, string phone, string address)
    {
        var command = new UpdateCustomerCommand(id, name, phone, address);
        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetCustomersQueryValidator_Should_Pass_For_Valid_Pagination()
    {
        var query = new GetCustomersQuery(new CustomerQueryParams(null, null, 1, 10));
        var result = _queryValidator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void GetCustomersQueryValidator_Should_Fail_For_Invalid_Pagination(int pageNumber, int pageSize)
    {
        var query = new GetCustomersQuery(new CustomerQueryParams(null, null, pageNumber, pageSize));
        var result = _queryValidator.Validate(query);
        result.IsValid.Should().BeFalse();
    }
}
