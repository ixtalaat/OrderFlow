using FluentValidation;

namespace OrderFlow.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().Must(x => x.Count <= 100);
        RuleForEach(x => x.Items).ChildRules(item => { item.RuleFor(x => x.ProductId).GreaterThan(0); item.RuleFor(x => x.Quantity).GreaterThan(0); });
    }
}
