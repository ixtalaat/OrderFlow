using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetCatalogProducts;

public sealed class GetCatalogProductsQueryHandler(IProductRepository products) : IRequestHandler<GetCatalogProductsQuery, Result<PagedList<CatalogProductResponse>>>
{
    public async Task<Result<PagedList<CatalogProductResponse>>> Handle(GetCatalogProductsQuery query, CancellationToken cancellationToken)
        => Result.Success(await products.GetCatalogPagedListAsync(query.QueryParams, cancellationToken));
}
