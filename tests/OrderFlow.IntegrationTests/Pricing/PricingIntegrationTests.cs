using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Pricing;

public sealed class PricingIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Wholesale_Customer_Order_Should_Use_Pricing_Rule_And_Snapshot_Price()
    {
        var (_, staffToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", staffToken);
        var email = $"tier-{Guid.NewGuid()}@test.com";
        var customer = await (await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Wholesale Customer", email, "Password@123", "+123", "Address"))).Content.ReadFromJsonAsync<CustomerResponse>();
        (await _client.PatchAsync($"/api/customers/{customer!.Id}/tier?tier=Wholesale", null)).IsSuccessStatusCode.Should().BeTrue();
        var product = await (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Tiered Product", "Description", $"TIER-{Guid.NewGuid():N}"[..12], 100, "Pricing"))).Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new { quantity = 2 });
        var (_, adminToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var rule = new { productId = product.Id, tier = CustomerTier.Wholesale, discountPercentage = 10, validFromUtc = DateTime.UtcNow.AddMinutes(-1) };
        (await _client.PostAsJsonAsync("/api/pricing-rules", rule)).IsSuccessStatusCode.Should().BeTrue();

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password@123"))).Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var response = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 1) }));
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        order!.Items[0].UnitPrice.Should().Be(90);
        order.TotalAmount.Should().Be(90);
    }
}
