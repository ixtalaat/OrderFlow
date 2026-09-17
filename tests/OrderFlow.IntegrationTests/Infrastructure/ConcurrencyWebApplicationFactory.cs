using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.IntegrationTests.Infrastructure;

// File-based SQLite factory for concurrency scenarios. The shared
// single-connection in-memory factory cannot execute parallel write
// requests (one connection handle cannot run overlapping transactions),
// while a file database gives every request its own connection and
// blocks short writers like SQL Server row locks do. The loser's
// optimistic-concurrency check then fails cleanly with 409 Conflict.
public sealed class ConcurrencyWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"orderflow-concurrency-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
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

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_dbPath};Default Timeout=30");
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TryDelete(_dbPath);
            TryDelete(_dbPath + "-journal");
            TryDelete(_dbPath + "-wal");
        }

        base.Dispose(disposing);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException)
        {
        }
    }
}
