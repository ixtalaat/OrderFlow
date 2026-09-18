using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.RateLimiting;

public sealed class RateLimitTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Catalog_Should_Return_TooManyRequests_When_Limit_Exceeded()
    {
        using var configured = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("RateLimiting:Catalog:PermitLimit", "1"));
        using var client = configured.CreateClient();
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(configured, "Customer");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        (await client.GetAsync("/api/catalog/products")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/catalog/products")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
