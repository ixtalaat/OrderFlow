using System.Collections.Concurrent;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var invoices = new ConcurrentDictionary<string, string>();
var app = builder.Build();

app.MapPost("/api/invoices", async (HttpRequest request, CancellationToken ct) =>
{
    var mode = request.Query["mode"].ToString();
    if (mode.Equals("timeout", StringComparison.OrdinalIgnoreCase))
        await Task.Delay(TimeSpan.FromMinutes(1), ct);
    if (mode.Equals("fail", StringComparison.OrdinalIgnoreCase))
        return Results.StatusCode(StatusCodes.Status500InternalServerError);

    using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: ct);
    var key = document.RootElement.GetProperty("idempotencyKey").GetString();
    if (string.IsNullOrWhiteSpace(key)) return Results.BadRequest();
    var invoiceId = invoices.GetOrAdd(key, _ => $"INV-{Guid.NewGuid():N}");
    return Results.Ok(new { invoiceId });
});

app.Run();
