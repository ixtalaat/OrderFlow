using OrderFlow.Application.Accounting;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Accounting;

public interface IAccountingService
{
    Task<AccountingInvoiceResult> CreateInvoiceAsync(OrderResponse order, string idempotencyKey, CancellationToken cancellationToken = default);
}
