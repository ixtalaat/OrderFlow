using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Accounting;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Infrastructure.Accounting;

public sealed class AccountingService(HttpClient client, IOptions<AccountingOptions> options, ILogger<AccountingService> logger) : IAccountingService
{
    public async Task<AccountingInvoiceResult> CreateInvoiceAsync(OrderResponse order, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            orderId = order.Id,
            idempotencyKey,
            totalAmount = order.TotalAmount,
            items = order.Items.Select(x => new { productId = x.ProductId, quantity = x.Quantity, unitPrice = x.UnitPrice, lineTotal = x.LineTotal })
        };

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var response = await client.PostAsJsonAsync("api/invoices", request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AccountingInvoiceResult>(cancellationToken);
                    return result ?? throw new InvalidOperationException("Accounting returned an empty invoice response.");
                }

                if ((int)response.StatusCode < 500)
                    throw new InvalidOperationException($"Accounting returned {(int)response.StatusCode}.");
                if (attempt > options.Value.RetryCount)
                    throw new HttpRequestException($"Accounting returned {(int)response.StatusCode}.");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && attempt <= options.Value.RetryCount && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Accounting invoice attempt {Attempt} failed for order {OrderId}.", attempt, order.Id);
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
            }
        }
    }
}
