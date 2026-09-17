using FluentValidation;
using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Products.DTOs;

namespace OrderFlow.Application.Products;

public sealed record GetProductByIdQuery(int Id, bool ActiveOnly = true) : IRequest<Result<ProductResponse>>;
public sealed record GetProductsQuery(ProductQueryParams QueryParams, bool ActiveOnly = true) : IRequest<Result<PagedList<ProductResponse>>>;

public sealed class GetProductByIdQueryHandler(IProductRepository products) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await products.GetResponseByIdAsync(query.Id, query.ActiveOnly, cancellationToken);
        return result is null ? Result.Failure<ProductResponse>(ProductErrors.NotFound) : Result.Success(result);
    }
}

public sealed class GetProductsQueryHandler(IProductRepository products) : IRequestHandler<GetProductsQuery, Result<PagedList<ProductResponse>>>
{
    public async Task<Result<PagedList<ProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
        => Result.Success(await products.GetPagedListAsync(query.QueryParams, query.ActiveOnly, cancellationToken));
}

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.QueryParams.PageNumber).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageSize).InclusiveBetween(1, 100);
    }
}
