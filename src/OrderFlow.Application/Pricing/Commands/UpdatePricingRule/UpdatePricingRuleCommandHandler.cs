using MediatR;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Pricing.DTOs;

namespace OrderFlow.Application.Pricing.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleCommandHandler(IPricingRuleRepository rules, IUnitOfWork unitOfWork, IAuditService audit) : IRequestHandler<UpdatePricingRuleCommand, Result<PricingRuleResponse>>
{
    public async Task<Result<PricingRuleResponse>> Handle(UpdatePricingRuleCommand command, CancellationToken ct)
    {
        var result = unitOfWork is ITransactionalUnitOfWork tx
            ? await tx.ExecuteInSerializableTransactionAsync(() => Core(command, ct), ct)
            : await Core(command, ct);
        if (result.IsFailure) return result;
        await audit.LogAsync("PricingRuleUpdated", "PricingRule", command.Id.ToString(), $"{command.Tier}, {command.DiscountPercentage}%.", ct);
        await audit.TrySaveAsync(ct);
        return result;
    }

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
