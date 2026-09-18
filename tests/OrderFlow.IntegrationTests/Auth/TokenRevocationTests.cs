using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auth;

public sealed class TokenRevocationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Deactivated_Customer_Token_Should_Stop_Working()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"revoke-{Guid.NewGuid()}@test.com";
        var customer = await (await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Revoke Customer", email, "Password@123", "+123", "Address")))
            .Content.ReadFromJsonAsync<CustomerResponse>();

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<AuthResponse>();

        using var before = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        before.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        (await _client.SendAsync(before)).StatusCode.Should().Be(HttpStatusCode.OK);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        await _client.PatchAsync($"/api/customers/{customer!.Id}/deactivate", null);

        using var after = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        after.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        (await _client.SendAsync(after)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
