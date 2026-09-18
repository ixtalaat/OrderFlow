using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Customers.DTOs;
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

        await TestAuthHelper.ConfirmEmailAsync(
            factory,
            _client,
            email);

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
    public async Task Login_Should_Return_Unauthorized_When_Customer_Is_Deactivated()
    {
        var email = $"deactivated-{Guid.NewGuid()}@test.com";
        var password = "Password@123";
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(
            factory,
            "SalesEmployee");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", salesToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerRequest(
                "Deactivated Customer",
                email,
                password,
                "+1234567890",
                "123 Main St"));
        var customer = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        await _client.PatchAsync($"/api/customers/{customer!.Id}/deactivate", null);
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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