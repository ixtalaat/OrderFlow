using System.Net;
using FluentAssertions;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Health;

public sealed class CorsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Preflight_Should_Succeed_For_Configured_Origin()
    {
        using var configured = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Cors:AllowedOrigins:0", "https://shop.example.com"));
        using var client = configured.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/catalog/products");
        request.Headers.Add("Origin", "https://shop.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.GetValues("Access-Control-Allow-Origin")
            .Should().ContainSingle("https://shop.example.com");
    }

    [Fact]
    public async Task Preflight_Should_Not_Allow_Unconfigured_Origin()
    {
        using var configured = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Cors:AllowedOrigins:0", "https://shop.example.com"));
        using var client = configured.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/catalog/products");
        request.Headers.Add("Origin", "https://evil.example.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        response.Headers.Should().NotContain(x => x.Key == "Access-Control-Allow-Origin");
    }
}
