using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public sealed class RefreshTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<AuthResponse> LoginAsync(string email)
    {
        using var client = factory.CreateClient();
        return await TestAuthHelper.RegisterConfirmAndLoginAsync(factory, client, email);
    }

    [Fact]
    public async Task Refresh_Should_Issue_New_Tokens()
    {
        var auth = await LoginAsync($"refresh-{Guid.NewGuid()}@test.com");

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken!));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await response.Content.ReadFromJsonAsync<RefreshResponse>();

        refreshed!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(auth.RefreshToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        (await _client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Reused_Refresh_Token_Should_Revoke_Family()
    {
        var auth = await LoginAsync($"reuse-{Guid.NewGuid()}@test.com");
        var first = await (await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken!)))
            .Content.ReadFromJsonAsync<RefreshResponse>();

        var reuse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken!));
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var rotated = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(first!.RefreshToken));
        rotated.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoked_Refresh_Token_Should_Not_Refresh()
    {
        var auth = await LoginAsync($"revoke-{Guid.NewGuid()}@test.com");
        (await _client.PostAsJsonAsync("/api/auth/revoke", new RevokeRequest(auth.RefreshToken!)))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(auth.RefreshToken!));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unknown_Refresh_Token_Should_Return_Unauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest("unknown-token"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
