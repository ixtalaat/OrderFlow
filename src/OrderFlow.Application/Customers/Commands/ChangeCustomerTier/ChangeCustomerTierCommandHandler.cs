using MediatR;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.ChangeCustomerTier;

public sealed class ChangeCustomerTierCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork, IAuditService audit) : IRequestHandler<ChangeCustomerTierCommand, Result>
{
    public async Task<Result> Handle(ChangeCustomerTierCommand command, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, ct); if (customer is null) return Result.Failure(CustomerErrors.NotFound);
        customer.ChangeTier(command.Tier); await unitOfWork.SaveChangesAsync(ct);
        await audit.LogAsync("CustomerTierChanged", "Customer", command.CustomerId.ToString(), $"Tier set to {command.Tier}.", ct);
        await audit.TrySaveAsync(ct);
        return Result.Success();
    }
}
