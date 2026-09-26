using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Commands.SetProductImage;

public sealed class SetProductImageCommandHandler(IProductRepository products, IProductImageStorage images, IUnitOfWork unitOfWork) : IRequestHandler<SetProductImageCommand, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(SetProductImageCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        StoredImage stored;
        try
        {
            stored = await images.SaveAsync(command.Content, command.ContentType, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure<ProductResponse>(ProductErrors.InvalidImage);
        }

        var previous = product.ImageUrl;
        product.SetImageUrl(stored.RelativeUrl);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (previous is not null) images.Delete(previous);

        var response = await products.GetResponseByIdAsync(product.Id, activeOnly: false, cancellationToken);
        return response is null
            ? Result.Failure<ProductResponse>(ProductErrors.NotFound)
            : Result.Success(response);
    }
}
