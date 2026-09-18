using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public sealed class ConfirmationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_Should_Return_Forbidden_When_Email_Is_Not_Confirmed()
    {
        var email = $"unconfirmed-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password@123", "Test Customer"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password@123"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_Should_Succeed_After_Email_Confirmation()
    {
        var email = $"confirm-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password@123", "Test Customer"));
        await TestAuthHelper.ConfirmEmailAsync(factory, _client, email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password@123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Confirm_Should_Return_BadRequest_For_Invalid_Token()
    {
        var email = $"badtoken-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password@123", "Test Customer"));

        var response = await _client.PostAsJsonAsync(
            "/api/auth/confirm-email",
            new ConfirmEmailRequest(email, "not-a-real-token"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Resend_Should_Return_OK_Even_For_Unknown_Email()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/resend-confirmation",
            new ResendConfirmationRequest($"unknown-{Guid.NewGuid()}@test.com"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
