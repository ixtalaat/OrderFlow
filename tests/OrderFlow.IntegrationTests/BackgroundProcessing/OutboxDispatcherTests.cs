using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Inventory.DTOs;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.BackgroundProcessing;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Infrastructure;

namespace OrderFlow.IntegrationTests.BackgroundProcessing;

public sealed class OutboxDispatcherTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private sealed class FakeJobClient : IBackgroundJobClient
    {
        public List<Job> Created { get; } = new();

        public string Create(Job job, IState state)
        {
            Created.Add(job);
            return "job-1";
        }

        public bool ChangeState(string jobId, IState state, string? expectedState) => throw new NotImplementedException();

        public bool ChangeState(Job job, IState state) => throw new NotImplementedException();
    }

    [Fact]
    public async Task Dispatch_Should_Publish_Pending_Messages_And_Mark_Enqueued()
    {
        var client = new FakeJobClient();
        using var configured = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services.AddSingleton<IBackgroundJobClient>(client)));

        var orderId = await CreateSubmittedOrderAsync(configured);
        await SeedOutboxMessageAsync(configured, orderId);

        var dispatcher = new OutboxDispatcher(
            configured.Services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<OutboxDispatcher>.Instance);
        await dispatcher.DispatchBatchAsync(CancellationToken.None);

        client.Created.Should().ContainSingle(job =>
            job.Type == typeof(OrderNotificationJob) && (int)job.Args[0]! == orderId);
        using (var scope = configured.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var message = await db.OutboxMessages.AsNoTracking().SingleAsync(x => x.OrderId == orderId);
            message.ProcessedAtUtc.Should().NotBeNull();
            message.HangfireJobId.Should().Be("job-1");
            message.Attempts.Should().Be(1);
        }
    }

    [Fact]
    public async Task Scheduler_Should_Stage_Outbox_Row()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var orderId = await CreateSubmittedOrderAsync(factory);
        var order = (await orders.GetByIdAsync(orderId))!;

        new HangfireJobScheduler(db).EnqueueOrderNotification(order);
        await db.SaveChangesAsync();

        var staged = await db.OutboxMessages.AsNoTracking().Where(x => x.OrderId == orderId).ToListAsync();
        staged.Should().ContainSingle(x => x.MessageType == "OrderNotification" && x.ProcessedAtUtc == null);
    }

    private static async Task<int> CreateSubmittedOrderAsync(WebApplicationFactory<Program> target)
    {
        using var staffClient = target.CreateClient();
        var (_, salesToken) = await TestAuthHelper.CreateUserAndGetTokenAsync(target, "SalesEmployee");
        staffClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", salesToken);
        var email = $"outbox-{Guid.NewGuid()}@test.com";
        await staffClient.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Outbox Customer", email, "Password@123", "+123", "Address"));
        var product = await (await staffClient.PostAsJsonAsync("/api/products",
            new CreateProductRequest("Outbox Product", "Description", $"OUT-{Guid.NewGuid():N}"[..12], 10, "Outbox")))
            .Content.ReadFromJsonAsync<ProductResponse>();
        await staffClient.PostAsJsonAsync($"/api/inventory/{product!.Id}/add-stock",
            new InventoryQuantityRequest(3));

        using var customerClient = target.CreateClient();
        var auth = await (await customerClient.PostAsJsonAsync("/api/auth/login",
            new OrderFlow.Application.Auth.DTOs.LoginRequest(email, "Password@123")))
            .Content.ReadFromJsonAsync<OrderFlow.Application.Auth.DTOs.AuthResponse>();
        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var order = await (await customerClient.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new[] { new CreateOrderItemRequest(product.Id, 1) })))
            .Content.ReadFromJsonAsync<OrderResponse>();
        return order!.Id;
    }

    private static async Task SeedOutboxMessageAsync(WebApplicationFactory<Program> target, int orderId)
    {
        using var scope = target.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var order = (await orders.GetByIdAsync(orderId))!;
        db.OutboxMessages.Add(OutboxMessage.Create(
            "OrderNotification",
            System.Text.Json.JsonSerializer.Serialize(new { status = "Submitted" }),
            order));
        await db.SaveChangesAsync();
    }
}
