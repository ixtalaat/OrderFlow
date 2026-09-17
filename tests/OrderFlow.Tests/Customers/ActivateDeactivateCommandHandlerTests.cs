using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.Commands.ActivateCustomer;
using OrderFlow.Application.Customers.Commands.DeactivateCustomer;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Customers;

public class ActivateDeactivateCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Activate_Should_Return_NotFound_When_Customer_Missing()
    {
        // Arrange
        _customerRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var handler = new ActivateCustomerCommandHandler(_customerRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(new ActivateCustomerCommand(999), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activate_Should_Set_Active_And_Commit()
    {
        // Arrange
        var customer = Customer.Create("user-123", "+1234567890", "Address");
        customer.Deactivate();

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        var handler = new ActivateCustomerCommandHandler(_customerRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(new ActivateCustomerCommand(customer.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_Should_Return_NotFound_When_Customer_Missing()
    {
        // Arrange
        _customerRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var handler = new DeactivateCustomerCommandHandler(_customerRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(new DeactivateCustomerCommand(999), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deactivate_Should_Set_Inactive_And_Commit()
    {
        // Arrange
        var customer = Customer.Create("user-123", "+1234567890", "Address");

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        var handler = new DeactivateCustomerCommandHandler(_customerRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(new DeactivateCustomerCommand(customer.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.IsActive.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
