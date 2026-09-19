using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Dashboard;

public sealed class DashboardTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Staff_Can_View_Stats_And_All_Orders()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"dash-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Dash Customer", email, "Password@123", "+123", "Address"));
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Dash Product", "Description", $"DSH-{Guid.NewGuid():N}"[..12], 20, "Dashboard")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(5));

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await (await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 2) })))
            .Content.ReadFromJsonAsync<OrderResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var stats = await _client.GetFromJsonAsync<DashboardStatsResponse>("/api/dashboard/stats");
        stats!.TotalOrders.Should().BeGreaterThanOrEqualTo(1);
        stats.SubmittedOrders.Should().BeGreaterThanOrEqualTo(1);
        stats.TotalRevenue.Should().BeGreaterThanOrEqualTo(40m);
        stats.TotalProducts.Should().BeGreaterThanOrEqualTo(1);
        stats.TotalCustomers.Should().BeGreaterThanOrEqualTo(1);

        var orders = await _client.GetFromJsonAsync<PagedList<OrderResponse>>("/api/dashboard/orders?pageNumber=1&pageSize=10");
        orders!.Items.Should().Contain(x => x.Id == order!.Id);
    }

    [Fact]
    public async Task Customer_Cannot_Access_Dashboard()
    {
        var (_, customerToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Customer");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerToken);

        (await _client.GetAsync("/api/dashboard/stats")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _client.GetAsync("/api/dashboard/orders")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
