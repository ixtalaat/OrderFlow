using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.Commands.UpdateCustomer;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Customers;

public class UpdateCustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTests()
    {
        _handler = new UpdateCustomerCommandHandler(_customerRepository, _identityService, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Customer_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateCustomerCommand(999, "Jane Doe", "+1234567890", "456 Oak Ave");
        _customerRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Update_Customer_And_Commit_When_Valid()
    {
        // Arrange
        var customer = Customer.Create("user-123", "+1111111111", "Old Address");
        var command = new UpdateCustomerCommand(customer.Id, "Updated Jane", "+2222222222", "New Address");

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);
        _identityService.UpdateUserFullNameAsync("user-123", "Updated Jane", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var expectedResponse = new CustomerResponse(
            customer.Id,
            customer.UserId,
            "Updated Jane",
            "jane@example.com",
            "+2222222222",
            "New Address",
            true,
            customer.CreatedAtUtc,
            DateTime.UtcNow);

        _customerRepository.GetResponseByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedResponse);
        customer.PhoneNumber.Should().Be("+2222222222");
        customer.Address.Should().Be("New Address");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
