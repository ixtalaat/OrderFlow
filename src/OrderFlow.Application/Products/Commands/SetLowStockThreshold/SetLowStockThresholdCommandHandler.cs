using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products.Commands.SetLowStockThreshold;

public sealed class SetLowStockThresholdCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<SetLowStockThresholdCommand, Result>
{
    public async Task<Result> Handle(SetLowStockThresholdCommand command, CancellationToken ct)
    {
        if (command.ProductId <= 0) return Result.Failure(ProductErrors.NotFound);
        if (command.Threshold.HasValue && command.Threshold.Value < 0) return Result.Failure(ProductErrors.InvalidThreshold);
        var product = await products.GetByIdAsync(command.ProductId, ct);
        if (product is null) return Result.Failure(ProductErrors.NotFound);
        product.SetLowStockThreshold(command.Threshold);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
