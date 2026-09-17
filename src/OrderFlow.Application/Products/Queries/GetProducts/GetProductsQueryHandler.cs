using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(IProductRepository products) : IRequestHandler<GetProductsQuery, Result<PagedList<ProductResponse>>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        => Result.Success(await products.GetPagedListAsync(query.QueryParams, query.ActiveOnly, cancellationToken));
}
