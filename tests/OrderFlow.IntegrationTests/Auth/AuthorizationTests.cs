using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public class AuthorizationTests(
    CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client =
        factory.CreateClient();

    [Fact]
    public async Task Me_Should_Return_401_When_No_Token()
    {
        // Act
        var response =
            await _client.GetAsync(
                "/api/auth/me");

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_Should_Return_200_When_User_Is_Authenticated()
    {
        // Arrange
        var token =
            await RegisterAndGetTokenAsync(
                $"customer-{Guid.NewGuid()}@test.com");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act
        var response =
            await _client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_Endpoint_Should_Return_403_For_Customer()
    {
        // Arrange
        var token =
            await RegisterAndGetTokenAsync(
                $"customer-{Guid.NewGuid()}@test.com");

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/admin");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act
        var response =
            await _client.SendAsync(request);

        // Assert
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Forbidden);
    }

    private async Task<string> RegisterAndGetTokenAsync(
        string email)
    {
        var auth = await TestAuthHelper.RegisterConfirmAndLoginAsync(
            factory,
            _client,
            email);

        return auth.AccessToken;
    }
}