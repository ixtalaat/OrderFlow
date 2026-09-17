using FluentValidation;

namespace OrderFlow.Application.Products;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.QueryParams.PageNumber).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageSize).InclusiveBetween(1, 100);
    }
}
