using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Pricing.DTOs;

namespace OrderFlow.Application.Pricing.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleCommandHandler(IPricingRuleRepository rules, IUnitOfWork unitOfWork) : IRequestHandler<UpdatePricingRuleCommand, Result<PricingRuleResponse>>
{
    public Task<Result<PricingRuleResponse>> Handle(UpdatePricingRuleCommand command, CancellationToken ct)
        => unitOfWork is ITransactionalUnitOfWork tx
            ? tx.ExecuteInSerializableTransactionAsync(() => Core(command, ct), ct)
            : Core(command, ct);

    private async Task<Result<PricingRuleResponse>> Core(UpdatePricingRuleCommand command, CancellationToken ct)
    {
        var rule = await rules.GetByIdAsync(command.Id, ct);
        if (rule is null) return Result.Failure<PricingRuleResponse>(PricingErrors.NotFound);
        if (await rules.HasOverlappingRuleAsync(rule.ProductId, command.Tier, command.ValidFromUtc, command.ValidToUtc, rule.Id, ct)) return Result.Failure<PricingRuleResponse>(PricingErrors.Overlap);
        rule.Update(command.Tier, command.DiscountPercentage, command.ValidFromUtc, command.ValidToUtc);
        if (unitOfWork is not ITransactionalUnitOfWork) await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(new PricingRuleResponse(rule.Id, rule.ProductId, rule.Tier, rule.DiscountPercentage, rule.ValidFromUtc, rule.ValidToUtc));
    }
}
