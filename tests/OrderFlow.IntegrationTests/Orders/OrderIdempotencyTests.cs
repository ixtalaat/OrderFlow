using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace OrderFlow.IntegrationTests.Orders;

public sealed class OrderIdempotencyTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<(HttpClient CustomerClient, int ProductId)> SetupAsync(string prefix)
    {
        using var staffClient = factory.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        staffClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"{prefix}-{Guid.NewGuid()}@test.com";
        await staffClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest($"{prefix} Customer", email, "Password@123", "+123", "Address"));
        var product = await (await staffClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest($"{prefix} Product", "Description", $"{prefix.ToUpperInvariant()}-{Guid.NewGuid():N}"[..12], 10, "Idempotency")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await staffClient.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(5));

        using var loginClient = factory.CreateClient();
        var auth = await (await loginClient.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        var customerClient = factory.CreateClient();
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        return (customerClient, product.Id);
    }

    private static async Task<HttpResponseMessage> PostOrderAsync(HttpClient client, int productId, int quantity, string key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders");
        request.Headers.Add("Idempotency-Key", key);
        request.Content = JsonContent.Create(new CreateOrderRequest(new[] { new CreateOrderItemRequest(productId, quantity) }));
        return await client.SendAsync(request);
    }

    [Fact]
    public async Task Same_Key_Twice_Should_Create_Single_Order()
    {
        var (client, productId) = await SetupAsync("idem");
        using (client)
        {
            var key = Guid.NewGuid().ToString();
            var first = await PostOrderAsync(client, productId, 2, key);
            var second = await PostOrderAsync(client, productId, 2, key);

            first.StatusCode.Should().Be(HttpStatusCode.Created);
            second.StatusCode.Should().Be(HttpStatusCode.Created);
            var firstBody = await first.Content.ReadAsStringAsync();
            var secondBody = await second.Content.ReadAsStringAsync();
            secondBody.Should().Be(firstBody);

            var history = await client.GetFromJsonAsync<OrderFlow.Application.Common.Models.PagedList<OrderResponse>>("/api/orders");
            history!.TotalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task Same_Key_With_Different_Payload_Should_Return_Unprocessable()
    {
        var (client, productId) = await SetupAsync("idem-mismatch");
        using (client)
        {
            var key = Guid.NewGuid().ToString();
            (await PostOrderAsync(client, productId, 1, key)).StatusCode.Should().Be(HttpStatusCode.Created);
            var mismatch = await PostOrderAsync(client, productId, 2, key);
            mismatch.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
    }

    [Fact]
    public async Task Expired_Key_Should_Create_New_Order()
    {
        var (client, productId) = await SetupAsync("idem-expired");
        using (client)
        {
            var key = Guid.NewGuid().ToString();
            (await PostOrderAsync(client, productId, 1, key)).StatusCode.Should().Be(HttpStatusCode.Created);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stored = await db.IdempotencyKeys.FirstAsync(x => x.Key == key);
                typeof(OrderFlow.Domain.Entities.IdempotencyKey)
                    .GetProperty(nameof(OrderFlow.Domain.Entities.IdempotencyKey.ExpiresAtUtc))!
                    .SetValue(stored, DateTime.UtcNow.AddMinutes(-1));
                await db.SaveChangesAsync();
            }

            (await PostOrderAsync(client, productId, 1, key)).StatusCode.Should().Be(HttpStatusCode.Created);
            var history = await client.GetFromJsonAsync<OrderFlow.Application.Common.Models.PagedList<OrderResponse>>("/api/orders");
            history!.TotalCount.Should().Be(2);
        }
    }
}
