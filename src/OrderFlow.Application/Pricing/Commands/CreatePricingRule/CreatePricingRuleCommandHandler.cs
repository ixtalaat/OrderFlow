using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Pricing.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Commands.CreatePricingRule;

public sealed class CreatePricingRuleCommandHandler(IPricingRuleRepository rules, IUnitOfWork unitOfWork) : IRequestHandler<CreatePricingRuleCommand, Result<PricingRuleResponse>>
{
    public async Task<Result<PricingRuleResponse>> Handle(CreatePricingRuleCommand command, CancellationToken ct)
    {
        if (await rules.HasOverlappingRuleAsync(command.ProductId, command.Tier, command.ValidFromUtc, command.ValidToUtc, cancellationToken: ct)) return Result.Failure<PricingRuleResponse>(PricingErrors.Overlap);
        var rule = PricingRule.Create(command.ProductId, command.Tier, command.DiscountPercentage, command.ValidFromUtc, command.ValidToUtc);
        await rules.AddAsync(rule, ct); await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new PricingRuleResponse(rule.Id, rule.ProductId, rule.Tier, rule.DiscountPercentage, rule.ValidFromUtc, rule.ValidToUtc));
    }
}
