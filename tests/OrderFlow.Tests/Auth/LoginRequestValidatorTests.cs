using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Auth.Validators;

namespace OrderFlow.Tests.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task Should_Pass_When_Request_Is_Valid()
    {
        // Arrange
        var request = new LoginRequest(
            "customer@test.com",
            "Password@123"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Fail_When_Email_Is_Empty()
    {
        // Arrange
        var request = new LoginRequest(
            string.Empty,
            "Password@123"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Email_Is_Invalid()
    {
        // Arrange
        var request = new LoginRequest(
            "invalid-email",
            "Password@123"
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Password_Is_Empty()
    {
        // Arrange
        var request = new LoginRequest(
            "customer@test.com",
            string.Empty
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}