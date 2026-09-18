using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(IOrderRepository orders, IInventoryRepository inventories, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs, ILogger<CancelOrderCommandHandler> logger) : IRequestHandler<CancelOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct);
        if (order is null || order.CustomerId != command.CustomerId) return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        if (order.Status != Domain.Entities.OrderStatus.Submitted) return Result.Failure<OrderResponse>(OrderErrors.InvalidTransition);
        foreach (var item in order.Items) { var inventory = await inventories.GetByProductIdAsync(item.ProductId, ct); if (inventory is null) return Result.Failure<OrderResponse>(OrderErrors.InsufficientStock); inventory.ReleaseStock(item.Quantity); }
        order.Cancel(); backgroundJobs.EnqueueOrderNotification(order);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<OrderResponse>(OrderErrors.ConcurrencyConflict);
        }
        logger.LogInformation("Order {OrderId} cancelled by customer {CustomerId}.", order.Id, command.CustomerId);
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
