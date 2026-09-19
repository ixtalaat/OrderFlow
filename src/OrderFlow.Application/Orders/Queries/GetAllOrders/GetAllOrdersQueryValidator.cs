using FluentValidation;

namespace OrderFlow.Application.Orders.Queries.GetAllOrders;

public sealed class GetAllOrdersQueryValidator : AbstractValidator<GetAllOrdersQuery>
{
    public GetAllOrdersQueryValidator()
    {
        RuleFor(x => x.QueryParams.PageNumber).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageSize).InclusiveBetween(1, 100);
    }
}
