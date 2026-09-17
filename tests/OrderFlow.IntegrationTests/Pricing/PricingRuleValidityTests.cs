using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Pricing;

public sealed class PricingRuleValidityTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Active_Rules_Should_Exclude_Expired_And_Future_Rules_Latest_First()
    {
        using var staffClient = factory.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        staffClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var product = await (await staffClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Validity Product", "Description", $"VAL-{Guid.NewGuid():N}"[..12], 100, "Pricing")))
            .Content.ReadFromJsonAsync<ProductResponse>();

        var now = DateTime.UtcNow;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.PricingRules.AddRange(
                PricingRule.Create(product!.Id, CustomerTier.Wholesale, 5, now.AddDays(-30), now.AddDays(-1)),
                PricingRule.Create(product.Id, CustomerTier.Wholesale, 50, now.AddDays(1), now.AddDays(30)),
                PricingRule.Create(product.Id, CustomerTier.Wholesale, 10, now.AddDays(-10), now.AddDays(30)),
                PricingRule.Create(product.Id, CustomerTier.Wholesale, 20, now.AddDays(-1), null));
            await db.SaveChangesAsync();
        }

        using var verifyScope = factory.Services.CreateScope();
        var rules = verifyScope.ServiceProvider.GetRequiredService<IPricingRuleRepository>();
        var active = await rules.GetActiveAsync(new[] { product!.Id }, CustomerTier.Wholesale, DateTime.UtcNow);

        active.Select(x => x.DiscountPercentage).Should().Equal(20m, 10m);
    }
}
