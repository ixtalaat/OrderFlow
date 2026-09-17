using FluentAssertions;
using NSubstitute;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.Commands.CreateCustomer;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Tests.Customers;

public class CreateCustomerCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _handler = new CreateCustomerCommandHandler(_identityService, _customerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Email_Is_Not_Unique()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com", "Password@123", "+1234567890", "123 Main St");
        _identityService.IsEmailUniqueAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.EmailAlreadyExists);
        await _customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Identity_Creation_Fails()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com", "Password@123", "+1234567890", "123 Main St");
        _identityService.IsEmailUniqueAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);
        _identityService.CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Customer, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<(string UserId, string Email, string FullName)>(CustomerErrors.UserCreationFailed));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CustomerErrors.UserCreationFailed);
        await _customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Create_Customer_And_Commit_When_Request_Is_Valid()
    {
        // Arrange
        var command = new CreateCustomerCommand("John Doe", "john@example.com", "Password@123", "+1234567890", "123 Main St");
        _identityService.IsEmailUniqueAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);
        _identityService.CreateUserAsync(command.Email, command.Password, command.FullName, Roles.Customer, Arg.Any<CancellationToken>())
            .Returns(Result.Success(("user-guid-123", command.Email, command.FullName)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be("user-guid-123");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.PhoneNumber.Should().Be("+1234567890");
        result.Value.Address.Should().Be("123 Main St");
        result.Value.IsActive.Should().BeTrue();

        await _customerRepository.Received(1).AddAsync(Arg.Is<Customer>(c => c.UserId == "user-guid-123"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
