using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public class RegisterTests(
    CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client =
        factory.CreateClient();

    [Fact]
    public async Task Register_Should_Return_Success_When_Request_Is_Valid()
    {
        // Arrange
        var request = new RegisterRequest(
            $"customer-{Guid.NewGuid()}@test.com",
            "Password@123",
            "Test Customer");

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            request);

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
    public async Task Register_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        // Arrange
        var email =
            $"duplicate-{Guid.NewGuid()}@test.com";

        var request = new RegisterRequest(
            email,
            "Password@123",
            "Test Customer");

        // First registration
        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        firstResponse.EnsureSuccessStatusCode();

        // Act
        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        secondResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_Should_Return_BadRequest_When_Request_Is_Invalid()
    {
        // Arrange
        var request = new RegisterRequest(
            "invalid-email",
            "123",
            "");

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }
}