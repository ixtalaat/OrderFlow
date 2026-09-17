using Hangfire;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Accounting;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Orders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.BackgroundProcessing;

public sealed class AccountingSynchronizationJob(IOrderRepository orders, IAccountingService accounting, IUnitOfWork unitOfWork, ILogger<AccountingSynchronizationJob> logger)
{
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(int orderId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(orderId, ct) ?? throw new InvalidOperationException($"Order {orderId} was not found.");
        if (order.AccountingSyncStatus == AccountingSyncStatus.Succeeded)
        {
            logger.LogInformation("Accounting synchronization already completed for order {OrderId}.", orderId);
            return;
        }

        order.BeginAccountingAttempt();
        await unitOfWork.SaveChangesAsync(ct);
        try
        {
            var response = await accounting.CreateInvoiceAsync((await orders.GetResponseByIdAsync(orderId, ct))!, $"Order:{orderId}:Invoice", ct);
            order.MarkAccountingSucceeded(response.InvoiceId);
            await unitOfWork.SaveChangesAsync(ct);
            logger.LogInformation("Accounting invoice {InvoiceId} synchronized for order {OrderId}.", response.InvoiceId, orderId);
        }
        catch (Exception ex)
        {
            order.MarkAccountingFailed(ex.Message);
            await unitOfWork.SaveChangesAsync(ct);
            logger.LogError(ex, "Accounting synchronization failed for order {OrderId}.", orderId);
            throw;
        }
    }
}
