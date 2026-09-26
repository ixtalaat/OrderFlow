using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Persistence.Seed;

namespace OrderFlow.IntegrationTests.Products;

public class CatalogSeedTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    [Fact]
    public async Task Seed_Should_Create_Door_Accessories_Catalog_Once()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();

        await CatalogSeeder.SeedAsync(db);
        (await db.Products.CountAsync()).Should().BeGreaterThan(20);
        (await db.Categories.CountAsync()).Should().BeGreaterThanOrEqualTo(5);
        (await db.Products.Select(x => x.Price).ToListAsync()).Should().OnlyContain(p => p > 0);
        var stocked = await db.Inventories.CountAsync(x => x.Quantity > 0);
        stocked.Should().Be(await db.Products.CountAsync());

        await CatalogSeeder.SeedAsync(db);
        (await db.Products.CountAsync()).Should().Be(await db.Inventories.CountAsync(x => x.Quantity > 0));
    }

    public void Dispose() => _connection.Dispose();
}
