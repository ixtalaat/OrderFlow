using FluentValidation;

namespace OrderFlow.Application.Products.Queries.GetCatalogProducts;

public sealed class GetCatalogProductsQueryValidator : AbstractValidator<GetCatalogProductsQuery>
{
    public GetCatalogProductsQueryValidator()
    {
        RuleFor(x => x.QueryParams.PageNumber).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageSize).InclusiveBetween(1, 100);
    }
}
