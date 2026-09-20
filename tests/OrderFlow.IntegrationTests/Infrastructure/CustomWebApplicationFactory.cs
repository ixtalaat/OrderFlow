using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection(
            "DataSource=:memory:");

        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Keep the suite independent of production rate limits.
        builder.UseSetting("RateLimiting:Auth:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Catalog:PermitLimit", "100000");
        builder.UseSetting("RateLimiting:Orders:PermitLimit", "100000");

        builder.ConfigureServices(services =>
        {
            // Remove all existing DbContext and EF Core registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore") &&
                 !d.ServiceType.Namespace.Contains("Identity"))
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // SQL Server when requested (CI parity job), SQLite otherwise.
            // Without an explicit connection string, a Testcontainers-managed
            // server is started automatically (cleaned up by Ryuk).
            if (string.Equals(
                    Environment.GetEnvironmentVariable("ORDERFLOW_TEST_DATABASE"),
                    "SqlServer",
                    StringComparison.OrdinalIgnoreCase))
            {
                var connection = Environment.GetEnvironmentVariable("ORDERFLOW_TEST_SQLSERVER")
                    ?? SqlServerTestContainer.GetConnectionString();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(connection);
                });
                return;
            }

            // Register SQLite test database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}