using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Customers;

public sealed class CustomerErasureTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Erase_Should_Remove_PII_But_Keep_Order_History()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"erase-{Guid.NewGuid()}@test.com";
        var customer = await (await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Erase Customer", email, "Password@123", "+123", "Address")))
            .Content.ReadFromJsonAsync<CustomerResponse>();
        var product = await (await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Erase Product", "Description", $"ERS-{Guid.NewGuid():N}"[..12], 25, "Erasure")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(2));

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        var userId = auth!.UserId;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var order = await (await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 1) })))
            .Content.ReadFromJsonAsync<OrderResponse>();

        var (_, adminToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        (await _client.DeleteAsync($"/api/customers/{customer!.Id}/erase"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var erased = await _client.GetFromJsonAsync<CustomerResponse>($"/api/customers/{customer.Id}");
        erased!.Email.Should().Contain("deleted.local");
        erased.PhoneNumber.Should().Be("erased");
        erased.IsActive.Should().BeFalse();

        var history = await _client.GetFromJsonAsync<OrderResponse>($"/api/orders/{order!.Id}");
        history!.TotalAmount.Should().Be(25m);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.RefreshTokens.AsNoTracking().AnyAsync(x => x.UserId == userId)).Should().BeFalse();
            (await db.IdempotencyKeys.AsNoTracking().AnyAsync(x => x.UserId == userId)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task Erase_Should_Return_NotFound_For_Unknown_Customer()
    {
        var (_, adminToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        (await _client.DeleteAsync("/api/customers/999999/erase"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
