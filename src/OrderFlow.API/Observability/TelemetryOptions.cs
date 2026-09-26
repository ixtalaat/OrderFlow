namespace OrderFlow.API.Observability;

public sealed class TelemetryOptions
{
    public const string SectionName = "Telemetry";
    public bool Enabled { get; set; } = true;
    public string? OtlpEndpoint { get; set; }
}
