using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Auditing;

public sealed class AuditTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> StaffTokenAsync()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        return token;
    }

    private async Task<int> AuditCountAsync(string action)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.AuditEntries.AsNoTracking().CountAsync(x => x.Action == action);
    }

    [Fact]
    public async Task Tier_Change_Should_Write_Audit_Entry_With_Actor()
    {
        var salesToken = await StaffTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"audit-{Guid.NewGuid()}@test.com";
        var customer = await (await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Audit Customer", email, "Password@123", "+123", "Address")))
            .Content.ReadFromJsonAsync<CustomerResponse>();
        var before = await AuditCountAsync("CustomerTierChanged");

        await _client.PatchAsync($"/api/customers/{customer!.Id}/tier?tier=Wholesale", null);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entry = await db.AuditEntries.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstAsync(x => x.Action == "CustomerTierChanged");
        (await AuditCountAsync("CustomerTierChanged")).Should().Be(before + 1);
        entry.EntityId.Should().Be(customer.Id.ToString());
        entry.ActorUserId.Should().NotBeNullOrWhiteSpace();
        entry.Details.Should().Contain("Wholesale");
    }

    [Fact]
    public async Task Adjust_Without_Reason_Should_Be_Rejected_And_Not_Audited()
    {
        var salesToken = await StaffTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Audit Product", "Description", $"AUD-{Guid.NewGuid():N}"[..12], 10, "Audit")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        var before = await AuditCountAsync("StockAdjusted");

        var response = await _client.PatchAsync(
            $"/api/inventory/{product!.Id}/adjust",
            JsonContent.Create(new { quantity = 1, reason = "" }));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await AuditCountAsync("StockAdjusted")).Should().Be(before);
    }

    [Fact]
    public async Task Adjust_With_Reason_Should_Write_Audit_Entry()
    {
        var salesToken = await StaffTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Audit Product Two", "Description", $"AU2-{Guid.NewGuid():N}"[..12], 10, "Audit")))
            .Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.PatchAsync(
            $"/api/inventory/{product!.Id}/adjust",
            JsonContent.Create(new { quantity = 2, reason = "Year-end count correction" }));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var entry = await db.AuditEntries.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstAsync(x => x.Action == "StockAdjusted");
        entry.EntityId.Should().Be(product.Id.ToString());
        entry.Details.Should().Contain("Year-end count correction");
    }
}
