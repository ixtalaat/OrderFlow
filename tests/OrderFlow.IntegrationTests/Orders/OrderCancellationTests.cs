using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Orders;

public sealed class OrderCancellationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string CustomerToken, string SalesToken, int ProductId)> SetupAsync(string prefix)
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"{prefix}-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest($"{prefix} Customer", email, "Password@123", "+123", "Address"));
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest($"{prefix} Product", "Description", $"{prefix.ToUpperInvariant()}-{Guid.NewGuid():N}"[..12], 10, "Cancellation")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(3));

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        return (auth!.AccessToken, salesToken, product.Id);
    }

    private async Task<OrderResponse> CreateOrderAsync(string customerToken, int productId)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        return (await (await client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(productId, 1) })))
            .Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    [Fact]
    public async Task Customer_Can_Cancel_Own_Submitted_Order_And_Release_Stock()
    {
        var (customerToken, salesToken, productId) = await SetupAsync("cancel");
        var order = await CreateOrderAsync(customerToken, productId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        var cancelled = await (await _client.PatchAsync($"/api/orders/{order.Id}/cancel", null))
            .Content.ReadFromJsonAsync<OrderResponse>();
        cancelled!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Cancelled);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var inventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{productId}");
        inventory!.ReservedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(3);
    }

    [Fact]
    public async Task Customer_Cannot_Cancel_Another_Customers_Order()
    {
        var (customerToken, salesToken, productId) = await SetupAsync("cancel-other");
        var order = await CreateOrderAsync(customerToken, productId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var otherEmail = $"cancel-other2-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Other Customer", otherEmail, "Password@123", "+123", "Address"));
        _client.DefaultRequestHeaders.Authorization = null;
        var otherAuth = await (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(otherEmail, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherAuth!.AccessToken);
        (await _client.PatchAsync($"/api/orders/{order.Id}/cancel", null))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_Cannot_Cancel_Confirmed_Order()
    {
        var (customerToken, salesToken, productId) = await SetupAsync("cancel-confirmed");
        var order = await CreateOrderAsync(customerToken, productId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        await _client.PatchAsync($"/api/orders/{order.Id}/confirm", null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        (await _client.PatchAsync($"/api/orders/{order.Id}/cancel", null))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Customer_Cannot_Cancel_Twice()
    {
        var (customerToken, _, productId) = await SetupAsync("cancel-twice");
        var order = await CreateOrderAsync(customerToken, productId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);
        (await _client.PatchAsync($"/api/orders/{order.Id}/cancel", null))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PatchAsync($"/api/orders/{order.Id}/cancel", null))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
