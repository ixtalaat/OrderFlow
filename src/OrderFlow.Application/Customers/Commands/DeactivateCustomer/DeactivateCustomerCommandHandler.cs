using MediatR;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerCommandHandler : IRequestHandler<DeactivateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public DeactivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IIdentityService identityService)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<Result> Handle(
        DeactivateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound);
        }

        customer.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _identityService.IncrementTokenVersionAsync(customer.UserId, cancellationToken);

        return Result.Success();
    }
}
