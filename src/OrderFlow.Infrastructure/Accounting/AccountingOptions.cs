namespace OrderFlow.Infrastructure.Accounting;

public sealed class AccountingOptions
{
    public const string SectionName = "Accounting";
    public string BaseUrl { get; set; } = "http://localhost:5099/";
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 2;
}
