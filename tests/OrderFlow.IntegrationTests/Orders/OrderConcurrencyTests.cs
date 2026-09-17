using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Inventory.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Orders;

public sealed class OrderConcurrencyTests(ConcurrencyWebApplicationFactory factory) : IClassFixture<ConcurrencyWebApplicationFactory>
{
    [Fact]
    public async Task Concurrent_Orders_For_Last_Stock_Should_Not_Oversell()
    {
        using var setupClient = factory.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        setupClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);

        var product = await (await setupClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Race Product", "Description", $"RACE-{Guid.NewGuid():N}"[..12], 10, "Concurrency")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await setupClient.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new InventoryQuantityRequest(5));

        var emailA = $"race-a-{Guid.NewGuid()}@test.com";
        var emailB = $"race-b-{Guid.NewGuid()}@test.com";
        await setupClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Race Customer A", emailA, "Password@123", "+123", "Address"));
        await setupClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Race Customer B", emailB, "Password@123", "+123", "Address"));

        using var clientA = factory.CreateClient();
        using var clientB = factory.CreateClient();
        var authA = await (await clientA.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(emailA, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        var authB = await (await clientB.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(emailB, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA!.AccessToken);
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB!.AccessToken);

        var payload = new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 5) });
        var results = await Task.WhenAll(
            clientA.PostAsJsonAsync("/api/orders", payload),
            clientB.PostAsJsonAsync("/api/orders", payload));

        var bodies = await Task.WhenAll(results.Select(r => r.Content.ReadAsStringAsync()));
        var diagnostic = string.Join(" | ", results.Select((r, i) => $"{r.StatusCode}: {bodies[i]}"));
        var statusCodes = results.Select(r => r.StatusCode).ToList();
        statusCodes.Should().Contain(HttpStatusCode.Created, because: diagnostic);
        statusCodes.Should().Contain(HttpStatusCode.Conflict, because: diagnostic);
        statusCodes.Count(s => s == HttpStatusCode.Created).Should().Be(1, because: diagnostic);

        setupClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var inventory = await setupClient.GetFromJsonAsync<InventoryResponse>($"/api/inventory/{product.Id}");
        inventory!.Quantity.Should().Be(5);
        inventory.ReservedQuantity.Should().Be(5);
        inventory.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Overlapping_Inventory_Reservations_Should_Conflict_On_Save()
    {
        using var setupClient = factory.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        setupClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);

        var product = await (await setupClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Token Product", "Description", $"TOK-{Guid.NewGuid():N}"[..12], 10, "Concurrency")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await setupClient.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new InventoryQuantityRequest(5));

        // Simulate two requests that both read Version N before either saves.
        using var scope1 = factory.Services.CreateScope();
        using var scope2 = factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var first = await db1.Inventories.SingleAsync(x => x.ProductId == product.Id);
        var second = await db2.Inventories.SingleAsync(x => x.ProductId == product.Id);

        first.ReserveStock(5);
        await db1.SaveChangesAsync();

        second.ReserveStock(5);
        var conflict = async () => await db2.SaveChangesAsync();
        await conflict.Should().ThrowAsync<DbUpdateConcurrencyException>();

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await verifyDb.Inventories.AsNoTracking().SingleAsync(x => x.ProductId == product.Id);
        stored.ReservedQuantity.Should().Be(5);
        stored.AvailableQuantity.Should().Be(0);
    }
}
