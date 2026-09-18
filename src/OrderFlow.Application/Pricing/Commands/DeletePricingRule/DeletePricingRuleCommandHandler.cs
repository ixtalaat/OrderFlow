using MediatR;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Pricing.Commands.DeletePricingRule;

public sealed class DeletePricingRuleCommandHandler(IPricingRuleRepository rules, IUnitOfWork unitOfWork, IAuditService audit) : IRequestHandler<DeletePricingRuleCommand, Result>
{
    public async Task<Result> Handle(DeletePricingRuleCommand command, CancellationToken ct)
    {
        var rule = await rules.GetByIdAsync(command.Id, ct); if (rule is null) return Result.Failure(PricingErrors.NotFound);
        rules.Remove(rule); await unitOfWork.SaveChangesAsync(ct);
        await audit.LogAsync("PricingRuleDeleted", "PricingRule", command.Id.ToString(), null, ct);
        await audit.TrySaveAsync(ct);
        return Result.Success();
    }
}
