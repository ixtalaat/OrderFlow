using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace OrderFlow.IntegrationTests.Infrastructure;

/// <summary>
/// Self-managed SQL Server for provider-parity runs. When
/// ORDERFLOW_TEST_DATABASE=SqlServer and no explicit connection string is
/// configured, the suite starts its own container instead of requiring an
/// external server, so local runs and CI behave identically.
/// </summary>
public static class SqlServerTestContainer
{
    private static readonly Lazy<Task<string>> ConnectionString = new(CreateAsync);

    public static string GetConnectionString() => ConnectionString.Value.GetAwaiter().GetResult();

    private static async Task<string> CreateAsync()
    {
        var container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
        await container.StartAsync();
        var builder = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = "OrderFlowTests"
        };
        return builder.ToString();
    }
}
