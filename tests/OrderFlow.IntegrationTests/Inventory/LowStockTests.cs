using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Inventory;

public sealed class LowStockTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Set_Threshold_Should_Accept_Valid_Value_And_Reject_Negative()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Threshold Product", "Description", $"THR-{Guid.NewGuid():N}"[..12], 10, "Inventory")))
            .Content.ReadFromJsonAsync<ProductResponse>();

        (await _client.PatchAsync($"/api/products/{product!.Id}/low-stock-threshold",
            JsonContent.Create(new LowStockThresholdRequest(3))))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.PatchAsync($"/api/products/{product.Id}/low-stock-threshold",
            JsonContent.Create(new LowStockThresholdRequest(-1))))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(5));
        var reserved = await _client.PostAsJsonAsync($"/api/inventory/{product.Id}/reserve",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(4));
        reserved.StatusCode.Should().Be(HttpStatusCode.OK);
        var inventory = await reserved.Content.ReadFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>();
        inventory!.AvailableQuantity.Should().Be(1);
    }
}
