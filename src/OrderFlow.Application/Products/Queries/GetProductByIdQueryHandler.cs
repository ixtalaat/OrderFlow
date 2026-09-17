using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products;

public sealed class GetProductByIdQueryHandler(IProductRepository products) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await products.GetResponseByIdAsync(query.Id, query.ActiveOnly, cancellationToken);
        return result is null ? Result.Failure<ProductResponse>(ProductErrors.NotFound) : Result.Success(result);
    }
}
