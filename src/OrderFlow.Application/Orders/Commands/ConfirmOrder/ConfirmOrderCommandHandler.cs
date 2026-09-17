using MediatR;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(IOrderRepository orders, IUnitOfWork unitOfWork, IBackgroundJobScheduler backgroundJobs) : IRequestHandler<ConfirmOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(ConfirmOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct);
        if (order is null) return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        try { order.Confirm(); } catch (InvalidOperationException) { return Result.Failure<OrderResponse>(OrderErrors.InvalidTransition); }
        await unitOfWork.SaveChangesAsync(ct);
        backgroundJobs.EnqueueOrderNotification(order.Id, order.Status);
        return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
