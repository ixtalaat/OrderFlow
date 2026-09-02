using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderFlow.API.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteResponseAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}