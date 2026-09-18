using MediatR;
using OrderFlow.Application.Auth;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders;

namespace OrderFlow.Application.Customers.Commands.EraseCustomer;

public sealed class EraseCustomerCommandHandler(
    ICustomerRepository customers,
    IIdentityService identity,
    IRefreshTokenRepository refreshTokens,
    IIdempotencyRepository idempotencyKeys,
    IAuditService audit,
    IUnitOfWork unitOfWork) : IRequestHandler<EraseCustomerCommand, Result>
{
    public async Task<Result> Handle(EraseCustomerCommand command, CancellationToken ct)
    {
        var customer = await customers.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
            return Result.Failure(CustomerErrors.NotFound);

        customer.Anonymize();
        await identity.AnonymizeUserAsync(customer.UserId, ct);
        await refreshTokens.RemoveByUserIdAsync(customer.UserId, ct);
        await idempotencyKeys.RemoveByUserIdAsync(customer.UserId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await audit.LogAsync("CustomerErased", "Customer", command.CustomerId.ToString(), null, ct);
        await audit.TrySaveAsync(ct);
        return Result.Success();
    }
}
