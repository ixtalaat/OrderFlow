using FluentValidation;

namespace OrderFlow.Application.Orders.Queries.GetCustomerOrders;

public sealed class GetCustomerOrdersQueryValidator : AbstractValidator<GetCustomerOrdersQuery>
{
    public GetCustomerOrdersQueryValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageNumber).GreaterThan(0);
        RuleFor(x => x.QueryParams.PageSize).InclusiveBetween(1, 100);
    }
}
