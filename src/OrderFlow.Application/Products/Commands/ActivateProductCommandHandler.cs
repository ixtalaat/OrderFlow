using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products;

public sealed class ActivateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<ActivateProductCommand, Result>
{
    public async Task<Result> Handle(ActivateProductCommand command, CancellationToken cancellationToken) { var product = await products.GetByIdAsync(command.Id, cancellationToken); if (product is null) return Result.Failure(ProductErrors.NotFound); product.Activate(); await unitOfWork.SaveChangesAsync(cancellationToken); return Result.Success(); }
}
