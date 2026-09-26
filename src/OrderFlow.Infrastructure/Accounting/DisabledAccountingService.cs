using Microsoft.Extensions.Logging;
using OrderFlow.Application.Accounting;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Infrastructure.Accounting;

public sealed class DisabledAccountingService(ILogger<DisabledAccountingService> logger) : IAccountingService
{
    public Task<AccountingInvoiceResult> CreateInvoiceAsync(OrderResponse order, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Accounting provider is Disabled; recording synthetic invoice for order {OrderId}.", order.Id);
        return Task.FromResult(new AccountingInvoiceResult($"DISABLED-{order.Id}"));
    }
}
