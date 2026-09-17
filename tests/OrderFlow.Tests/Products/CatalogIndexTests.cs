using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Tests.Products;

public sealed class CatalogIndexTests
{
    [Fact]
    public void Catalog_Supporting_Indexes_Should_Exist()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(local);Database=OrderFlowIndexProbe;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var db = new ApplicationDbContext(options);

        var productIndexes = db.Model.FindEntityType(typeof(Product))!
            .GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name)));
        productIndexes.Should().Contain("IsActive,Name");

        var pricingRuleIndexes = db.Model.FindEntityType(typeof(PricingRule))!
            .GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name)));
        pricingRuleIndexes.Should().Contain("ProductId,Tier,ValidFromUtc");
    }
}
