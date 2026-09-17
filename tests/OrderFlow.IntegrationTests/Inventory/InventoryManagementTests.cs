using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Inventory.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Inventory;

public sealed class InventoryManagementTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task SalesEmployee_Can_Manage_Inventory_Lifecycle()
    {
        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Stock Product", "Description", $"INV-{Guid.NewGuid():N}"[..12], 10, "Inventory"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();

        var add = await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new InventoryQuantityRequest(10));
        (await add.Content.ReadFromJsonAsync<InventoryResponse>())!.AvailableQuantity.Should().Be(10);
        var reserve = await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/reserve", new InventoryQuantityRequest(4));
        var reserved = await reserve.Content.ReadFromJsonAsync<InventoryResponse>();
        reserved!.ReservedQuantity.Should().Be(4);
        reserved.AvailableQuantity.Should().Be(6);
        await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/release", new InventoryQuantityRequest(1));
        var confirm = await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/confirm", new InventoryQuantityRequest(2));
        var confirmed = await confirm.Content.ReadFromJsonAsync<InventoryResponse>();
        confirmed!.Quantity.Should().Be(8);
        confirmed.ReservedQuantity.Should().Be(1);
    }

    [Fact]
    public async Task Inventory_Should_Reject_Insufficient_Stock_And_Unauthorized_Access()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        (await _client.GetAsync("/api/inventory/1")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var (_, token) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var create = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Limited Product", "Description", $"LIM-{Guid.NewGuid():N}"[..12], 10, "Inventory"));
        var product = await create.Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new InventoryQuantityRequest(1));
        var response = await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/reserve", new InventoryQuantityRequest(2));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
