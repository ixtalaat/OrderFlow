using System.Net;
using System.Text.Json;
using FluentAssertions;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Health;

public sealed class HealthCheckTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Readiness_Probe_Should_Report_Database_Healthy()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        document.RootElement.GetProperty("checks").EnumerateArray()
            .Should().ContainSingle(x =>
                x.GetProperty("name").GetString() == "database" &&
                x.GetProperty("status").GetString() == "Healthy");
    }

    [Fact]
    public async Task Liveness_Probe_Should_Succeed_Without_Checks()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Correlation_Id_Should_Be_Echoed_When_Provided()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", "test-correlation-123");
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Correlation-ID").Should().ContainSingle("test-correlation-123");
    }

    [Fact]
    public async Task Correlation_Id_Should_Be_Generated_When_Missing()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var correlationId = response.Headers.GetValues("X-Correlation-ID").Should().ContainSingle().Subject;
        correlationId.Should().NotBeNullOrWhiteSpace();
    }
}
