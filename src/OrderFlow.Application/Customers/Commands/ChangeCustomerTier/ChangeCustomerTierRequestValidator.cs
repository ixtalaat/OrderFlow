using FluentValidation;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Customers.Commands.ChangeCustomerTier;

public sealed class ChangeCustomerTierRequestValidator : AbstractValidator<ChangeCustomerTierRequest>
{
    public ChangeCustomerTierRequestValidator()
    {
        RuleFor(x => x.Tier)
            .Must(value => Enum.TryParse<CustomerTier>(value, true, out _))
            .WithMessage("Tier must be Regular, Wholesale, or Vip.");
    }
}
