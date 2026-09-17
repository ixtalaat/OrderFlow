using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Accounting;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Accounting;

namespace OrderFlow.Tests.Accounting;

public sealed class AccountingServiceTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();
        public List<string?> IdempotencyKeys { get; } = new();

        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> response) => _responses.Enqueue(response);

        public void EnqueueThrow(Exception exception) =>
            _responses.Enqueue(_ => throw exception);

        public void EnqueueStatus(HttpStatusCode status, object? payload = null) =>
            _responses.Enqueue(_ => new HttpResponseMessage(status)
            {
                Content = payload is null
                    ? null
                    : new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            });

        public void EnqueueRaw(HttpStatusCode status, string body) =>
            _responses.Enqueue(_ => new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            IdempotencyKeys.Add(body is null
                ? null
                : JsonDocument.Parse(body).RootElement.GetProperty("idempotencyKey").GetString());
            return _responses.Dequeue()(request);
        }
    }

    private static readonly OrderResponse Order = new(
        42, 7, OrderStatus.Confirmed, 50m, DateTime.UtcNow,
        new[] { new OrderItemResponse(3, "Product", 5, 10m, 50m) },
        AccountingSyncStatus.Pending, null, 0, null, null);

    private static AccountingService CreateService(StubHandler handler, int retryCount = 2) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") },
            Options.Create(new AccountingOptions { BaseUrl = "http://localhost/", TimeoutSeconds = 10, RetryCount = retryCount }),
            NullLogger<AccountingService>.Instance);

    [Fact]
    public async Task Should_Return_Invoice_On_Success()
    {
        var handler = new StubHandler();
        handler.EnqueueStatus(HttpStatusCode.OK, new AccountingInvoiceResult("INV-1"));

        var result = await CreateService(handler).CreateInvoiceAsync(Order, "key-1");

        result.InvoiceId.Should().Be("INV-1");
        handler.IdempotencyKeys.Should().ContainSingle("key-1");
    }

    [Fact]
    public async Task Should_Retry_With_Same_Idempotency_Key_After_Server_Error()
    {
        var handler = new StubHandler();
        handler.EnqueueStatus(HttpStatusCode.InternalServerError);
        handler.EnqueueStatus(HttpStatusCode.OK, new AccountingInvoiceResult("INV-2"));

        var result = await CreateService(handler).CreateInvoiceAsync(Order, "key-2");

        result.InvoiceId.Should().Be("INV-2");
        handler.IdempotencyKeys.Should().Equal("key-2", "key-2");
    }

    [Fact]
    public async Task Should_Throw_After_Retries_Exhausted()
    {
        var handler = new StubHandler();
        handler.EnqueueStatus(HttpStatusCode.InternalServerError);
        handler.EnqueueStatus(HttpStatusCode.BadGateway);
        handler.EnqueueStatus(HttpStatusCode.ServiceUnavailable);

        var action = () => CreateService(handler, retryCount: 2).CreateInvoiceAsync(Order, "key-3");

        await action.Should().ThrowAsync<HttpRequestException>();
        handler.IdempotencyKeys.Should().Equal("key-3", "key-3", "key-3");
    }

    [Fact]
    public async Task Should_Not_Retry_Client_Errors()
    {
        var handler = new StubHandler();
        handler.EnqueueStatus(HttpStatusCode.BadRequest);

        var action = () => CreateService(handler).CreateInvoiceAsync(Order, "key-4");

        await action.Should().ThrowAsync<InvalidOperationException>();
        handler.IdempotencyKeys.Should().ContainSingle("key-4");
    }

    [Fact]
    public async Task Should_Retry_After_Timeout()
    {
        var handler = new StubHandler();
        handler.EnqueueThrow(new TaskCanceledException("Timed out."));
        handler.EnqueueStatus(HttpStatusCode.OK, new AccountingInvoiceResult("INV-5"));

        var result = await CreateService(handler).CreateInvoiceAsync(Order, "key-5");

        result.InvoiceId.Should().Be("INV-5");
        handler.IdempotencyKeys.Should().Equal("key-5", "key-5");
    }

    [Fact]
    public async Task Should_Throw_On_Empty_Invoice_Response()
    {
        var handler = new StubHandler();
        handler.EnqueueRaw(HttpStatusCode.OK, "null");

        var action = () => CreateService(handler).CreateInvoiceAsync(Order, "key-6");

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
