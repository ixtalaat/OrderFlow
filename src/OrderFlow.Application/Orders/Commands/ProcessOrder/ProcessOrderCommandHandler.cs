using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders.DTOs;

namespace OrderFlow.Application.Orders.Commands.ProcessOrder;

public sealed class ProcessOrderCommandHandler(IOrderRepository orders, IUnitOfWork unitOfWork) : IRequestHandler<ProcessOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(ProcessOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(command.OrderId, ct); if (order is null) return Result.Failure<OrderResponse>(OrderErrors.NotFound);
        if (order.Status != Domain.Entities.OrderStatus.Confirmed) return Result.Failure<OrderResponse>(OrderErrors.InvalidTransition);
        order.Process(); await unitOfWork.SaveChangesAsync(ct); return Result.Success((await orders.GetResponseByIdAsync(order.Id, ct))!);
    }
}
