using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Products;

public sealed class DeactivateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork) : IRequestHandler<DeactivateProductCommand, Result>
{
    public async Task<Result> Handle(DeactivateProductCommand command, CancellationToken cancellationToken) { var product = await products.GetByIdAsync(command.Id, cancellationToken); if (product is null) return Result.Failure(ProductErrors.NotFound); product.Deactivate(); await unitOfWork.SaveChangesAsync(cancellationToken); return Result.Success(); }
}
