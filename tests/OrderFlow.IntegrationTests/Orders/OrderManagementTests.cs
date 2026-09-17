using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Orders;

public sealed class OrderManagementTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Customer_Can_Create_Order_And_Inventory_Is_Reserved()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"order-{Guid.NewGuid()}@test.com";
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Order Customer", email, "Password@123", "+123", "Address"));
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        var productResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Order Product", "Description", $"ORD-{Guid.NewGuid():N}"[..12], 10, "Orders"));
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new { quantity = 5 });

        _client.DefaultRequestHeaders.Authorization = null;
        var login = await _client.PostAsJsonAsync("/api/auth/login", new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123"));
        var auth = await login.Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 2) }));
        order.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await order.Content.ReadFromJsonAsync<OrderResponse>();
        result!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Submitted);
        result.TotalAmount.Should().Be(20);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var inventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{product.Id}");
        inventory!.AvailableQuantity.Should().Be(3);
        inventory.ReservedQuantity.Should().Be(2);
    }

    [Fact]
    public async Task Customer_Order_With_Partially_Insufficient_Stock_Should_Roll_Back_Entirely()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"rollback-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Rollback Customer", email, "Password@123", "+123", "Address"));
        var first = await (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Rollback Product One", "Description", $"RB1-{Guid.NewGuid():N}"[..12], 10, "Orders"))).Content.ReadFromJsonAsync<ProductResponse>();
        var second = await (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Rollback Product Two", "Description", $"RB2-{Guid.NewGuid():N}"[..12], 10, "Orders"))).Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{first!.Id}/add-stock", new { quantity = 10 });
        await _client.PostAsJsonAsync($"/api/inventory/{second!.Id}/add-stock", new { quantity = 1 });

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login", new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123"))).Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new[]
        {
            new CreateOrderItemRequest(first.Id, 2),
            new CreateOrderItemRequest(second.Id, 5)
        }));
        order.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var history = await _client.GetFromJsonAsync<OrderFlow.Application.Common.Models.PagedList<OrderResponse>>("/api/orders");
        history!.TotalCount.Should().Be(0);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var firstInventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{first.Id}");
        firstInventory!.ReservedQuantity.Should().Be(0);
        firstInventory.AvailableQuantity.Should().Be(10);
        var secondInventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{second.Id}");
        secondInventory!.ReservedQuantity.Should().Be(0);
        secondInventory.AvailableQuantity.Should().Be(1);
    }

    [Fact]
    public async Task Staff_Can_Drive_Order_Through_Full_Workflow()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"workflow-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Workflow Customer", email, "Password@123", "+123", "Address"));
        var product = await (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Workflow Product", "Description", $"WKF-{Guid.NewGuid():N}"[..12], 10, "Orders"))).Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new { quantity = 5 });

        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login", new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123"))).Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await (await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 2) }))).Content.ReadFromJsonAsync<OrderResponse>();
        order!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Submitted);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var confirmed = await (await _client.PatchAsync($"/api/orders/{order.Id}/confirm", null)).Content.ReadFromJsonAsync<OrderResponse>();
        confirmed!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Confirmed);
        var processing = await (await _client.PatchAsync($"/api/orders/{order.Id}/processing", null)).Content.ReadFromJsonAsync<OrderResponse>();
        processing!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Processing);
        var completed = await (await _client.PatchAsync($"/api/orders/{order.Id}/complete", null)).Content.ReadFromJsonAsync<OrderResponse>();
        completed!.Status.Should().Be(OrderFlow.Domain.Entities.OrderStatus.Completed);

        var inventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{product.Id}");
        inventory!.Quantity.Should().Be(3);
        inventory.ReservedQuantity.Should().Be(0);
        inventory.AvailableQuantity.Should().Be(3);
    }

    [Fact]
    public async Task SalesEmployee_Can_Reject_Order_And_Release_Inventory()
    {
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"reject-{Guid.NewGuid()}@test.com";
        var customer = await (await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Reject Customer", email, "Password@123", "+123", "Address"))).Content.ReadFromJsonAsync<CustomerResponse>();
        var product = await (await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Reject Product", "Description", $"REJ-{Guid.NewGuid():N}"[..12], 10, "Orders"))).Content.ReadFromJsonAsync<ProductResponse>();
        await _client.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock", new { quantity = 2 });
        _client.DefaultRequestHeaders.Authorization = null;
        var auth = await (await _client.PostAsJsonAsync("/api/auth/login", new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123"))).Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await (await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 1) }))).Content.ReadFromJsonAsync<OrderResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        await _client.PatchAsync($"/api/orders/{order!.Id}/reject", null);
        var inventory = await _client.GetFromJsonAsync<OrderFlow.Application.Inventory.DTOs.InventoryResponse>($"/api/inventory/{product.Id}");
        inventory!.AvailableQuantity.Should().Be(2);
        inventory.ReservedQuantity.Should().Be(0);
    }
}
