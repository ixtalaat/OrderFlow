using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Auth.Validators;

namespace OrderFlow.Tests.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task Should_Pass_When_Request_Is_Valid()
    {
        // Arrange
        var request = new RegisterRequest(
            "customer@test.com",
            "Password@123",
            "Test Customer"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Fail_When_Email_Is_Invalid()
    {
        // Arrange
        var request = new RegisterRequest(
            "invalid-email",
            "Password@123",
            "Test Customer"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Password_Is_Too_Short()
    {
        // Arrange
        var request = new RegisterRequest(
            "customer@test.com",
            "123",
            "Test Customer"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}