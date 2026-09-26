namespace OrderFlow.Infrastructure.Accounting;

public sealed class AccountingOptions
{
    public const string SectionName = "Accounting";

    /// <summary>
    /// Fake (default, local stub), GenericRest (any real REST vendor behind the
    /// same invoice contract), or Disabled (orders complete without invoicing).
    /// </summary>
    public string Provider { get; set; } = "Fake";
    public string BaseUrl { get; set; } = "http://localhost:5099/";
    public string InvoicesPath { get; set; } = "api/invoices";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 2;
}
