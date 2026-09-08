using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public class LoginTests(
    CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client =
        factory.CreateClient();

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Valid()
    {
        // Arrange
        var email =
            $"login-{Guid.NewGuid()}@test.com";

        var password =
            "Password@123";

        var registerRequest =
            new RegisterRequest(
                email,
                password,
                "Test Customer");

        await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerRequest);

        var loginRequest =
            new LoginRequest(
                email,
                password);

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginRequest);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        result.Should().NotBeNull();

        result!.AccessToken
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_When_Password_Is_Invalid()
    {
        // Arrange
        var email =
            $"invalid-password-{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(
                email,
                "Password@123",
                "Test Customer"));

        var request =
            new LoginRequest(
                email,
                "WrongPassword@123");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Should_Return_Unauthorized_When_User_Does_Not_Exist()
    {
        // Arrange
        var request =
            new LoginRequest(
                $"not-found-{Guid.NewGuid()}@test.com",
                "Password@123");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Request_Is_Invalid()
    {
        // Arrange
        var request =
            new LoginRequest(
                "invalid-email",
                "");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }
}