using MediatR;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Exceptions;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.CompleteOrder;

public sealed class CompleteOrderCommandHandler(IOrderRepository orders, IInventoryRepository inventories, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs) : IRequestHandler<CompleteOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CompleteOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct); if (order is null) return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        if (order.Status != Domain.Entities.OrderStatus.Processing) return Result.Failure<OrderResponse>(OrderErrors.InvalidTransition);
        foreach (var item in order.Items) { var inventory = await inventories.GetByProductIdAsync(item.ProductId, ct); if (inventory is null || item.Quantity > inventory.ReservedQuantity) return Result.Failure<OrderResponse>(OrderErrors.InsufficientStock); inventory.ConfirmStock(item.Quantity); }
        order.Complete(); backgroundJobs.EnqueueOrderNotification(order);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<OrderResponse>(OrderErrors.ConcurrencyConflict);
        }
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
