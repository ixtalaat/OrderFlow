using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.ChangeCustomerTier;

public sealed class ChangeCustomerTierCommandHandler(ICustomerRepository customers, IUnitOfWork unitOfWork) : IRequestHandler<ChangeCustomerTierCommand, Result>
{
    public async Task<Result> Handle(ChangeCustomerTierCommand command, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, ct); if (customer is null) return Result.Failure(CustomerErrors.NotFound);
        customer.ChangeTier(command.Tier); await unitOfWork.SaveChangesAsync(ct); return Result.Success();
    }
}
