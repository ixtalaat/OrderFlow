using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Coupons;

public sealed class CouponOrderTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string CustomerToken, int ProductId)> SetupAsync(string prefix)
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"{prefix}-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest($"{prefix} Customer", email, "Password@123", "+123", "Address"));
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest($"{prefix} Product", "Description", $"{prefix.ToUpperInvariant()}-{Guid.NewGuid():N}"[..12], 100, "Coupons")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(5));

        var (_, adminToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PostAsJsonAsync("/api/coupons", new
        {
            code = $"{prefix.ToUpperInvariant()}-10",
            discountPercentage = 10,
            minOrderTotal = 0,
            validFromUtc = DateTime.UtcNow.AddDays(-1),
            validToUtc = (DateTime?)DateTime.UtcNow.AddDays(30),
            maxRedemptions = (int?)null
        });

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        return (auth!.AccessToken, product.Id);
    }

    [Fact]
    public async Task Order_With_Valid_Coupon_Should_Apply_Discount()
    {
        var (customerToken, productId) = await SetupAsync("coupon");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        var order = await (await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(productId, 1) }, "COUPON-10")))
            .Content.ReadFromJsonAsync<OrderResponse>();

        order!.CouponCode.Should().Be("COUPON-10");
        order.DiscountAmount.Should().Be(10m);
        order.TotalAmount.Should().Be(90m);
    }

    [Fact]
    public async Task Order_With_Unknown_Coupon_Should_Return_BadRequest()
    {
        var (customerToken, productId) = await SetupAsync("coupon-bad");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(productId, 1) }, "NOPE"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
