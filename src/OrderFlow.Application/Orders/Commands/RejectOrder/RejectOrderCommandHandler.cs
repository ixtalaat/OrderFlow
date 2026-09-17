using MediatR;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.RejectOrder;

public sealed class RejectOrderCommandHandler(IOrderRepository orders, IInventoryRepository inventories, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs) : IRequestHandler<RejectOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(RejectOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct);
        if (order is null) return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        if (order.Status != Domain.Entities.OrderStatus.Submitted) return Result.Failure<OrderResponse>(OrderErrors.InvalidTransition);
        foreach (var item in order.Items) { var inventory = await inventories.GetByProductIdAsync(item.ProductId, ct); if (inventory is null) return Result.Failure<OrderResponse>(OrderErrors.InsufficientStock); inventory.ReleaseStock(item.Quantity); }
        order.Reject(); backgroundJobs.EnqueueOrderNotification(order); await unitOfWork.SaveChangesAsync(ct);
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
