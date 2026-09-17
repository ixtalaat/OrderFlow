using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProductById;

public sealed class GetCatalogProductByIdQueryHandler(IProductRepository products) : IRequestHandler<GetCatalogProductByIdQuery, Result<CatalogProductResponse>>
{
    public async Task<Result<CatalogProductResponse>> Handle(GetCatalogProductByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await products.GetCatalogResponseByIdAsync(query.Id, cancellationToken);
        return product is null ? Result.Failure<CatalogProductResponse>(ProductErrors.NotFound) : Result.Success(product);
    }
}
