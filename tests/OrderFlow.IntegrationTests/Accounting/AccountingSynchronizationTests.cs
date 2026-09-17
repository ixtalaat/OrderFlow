using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Application.Accounting;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.BackgroundProcessing;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.Accounting;

public sealed class AccountingSynchronizationTests(ConcurrencyWebApplicationFactory factory) : IClassFixture<ConcurrencyWebApplicationFactory>
{
    private sealed class StubAccountingService : IAccountingService
    {
        public Func<OrderResponse, string, CancellationToken, Task<AccountingInvoiceResult>>? Behavior;
        public List<string> IdempotencyKeys { get; } = new();

        public Task<AccountingInvoiceResult> CreateInvoiceAsync(OrderResponse order, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            IdempotencyKeys.Add(idempotencyKey);
            return Behavior!(order, idempotencyKey, cancellationToken);
        }
    }

    private async Task<int> CreateSubmittedOrderAsync()
    {
        using var staffClient = factory.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(factory, "SalesEmployee");
        staffClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"sync-{Guid.NewGuid()}@test.com";
        await staffClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Sync Customer", email, "Password@123", "+123", "Address"));
        var product = await (await staffClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Sync Product", "Description", $"SYN-{Guid.NewGuid():N}"[..12], 10, "Accounting")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await staffClient.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new OrderFlow.Application.Inventory.DTOs.InventoryQuantityRequest(5));

        using var customerClient = factory.CreateClient();
        var auth = await (await customerClient.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await (await customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 1) })))
            .Content.ReadFromJsonAsync<OrderResponse>();
        return order!.Id;
    }

    private async Task ExecuteJobAsync(int orderId, StubAccountingService stub)
    {
        using var scope = factory.Services.CreateScope();
        var job = new AccountingSynchronizationJob(
            scope.ServiceProvider.GetRequiredService<IOrderRepository>(),
            stub,
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            NullLogger<AccountingSynchronizationJob>.Instance);
        await job.ExecuteAsync(orderId);
    }

    private async Task<Order> ReadOrderAsync(int orderId)
    {
        using var scope = factory.Services.CreateScope();
        return (await scope.ServiceProvider.GetRequiredService<IOrderRepository>().GetByIdAsync(orderId))!;
    }

    [Fact]
    public async Task Successful_Sync_Should_Persist_Invoice_Id()
    {
        var orderId = await CreateSubmittedOrderAsync();
        var stub = new StubAccountingService
        {
            Behavior = (_, _, _) => Task.FromResult(new AccountingInvoiceResult("INV-99"))
        };

        await ExecuteJobAsync(orderId, stub);

        var order = await ReadOrderAsync(orderId);
        order.ExternalInvoiceId.Should().Be("INV-99");
        order.AccountingSyncStatus.Should().Be(AccountingSyncStatus.Succeeded);
        order.AccountingSyncAttempts.Should().BeGreaterThan(0);
        stub.IdempotencyKeys.Should().ContainSingle($"Order:{orderId}:Invoice");
    }

    [Fact]
    public async Task Failed_Sync_Should_Persist_Failure_And_Rethrow()
    {
        var orderId = await CreateSubmittedOrderAsync();
        var stub = new StubAccountingService
        {
            Behavior = (_, _, _) => throw new HttpRequestException("Accounting down.")
        };

        var action = () => ExecuteJobAsync(orderId, stub);

        await action.Should().ThrowAsync<HttpRequestException>();
        var order = await ReadOrderAsync(orderId);
        order.AccountingSyncStatus.Should().Be(AccountingSyncStatus.Failed);
        order.AccountingLastError.Should().Contain("Accounting down.");
    }

    [Fact]
    public async Task Already_Succeeded_Sync_Should_Skip_External_Call()
    {
        var orderId = await CreateSubmittedOrderAsync();
        var stub = new StubAccountingService
        {
            Behavior = (_, _, _) => Task.FromResult(new AccountingInvoiceResult("INV-1"))
        };
        await ExecuteJobAsync(orderId, stub);

        stub.Behavior = (_, _, _) => throw new HttpRequestException("Must not be called.");
        await ExecuteJobAsync(orderId, stub);

        var order = await ReadOrderAsync(orderId);
        order.ExternalInvoiceId.Should().Be("INV-1");
        order.AccountingSyncStatus.Should().Be(AccountingSyncStatus.Succeeded);
    }
}
