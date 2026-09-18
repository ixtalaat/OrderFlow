using MediatR;
using OrderFlow.Application.Common.Auditing;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.ActivateCustomer;

public sealed class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ActivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<Result> Handle(
        ActivateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound);
        }

        customer.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("CustomerActivated", "Customer", command.Id.ToString(), null, cancellationToken);
        await _auditService.TrySaveAsync(cancellationToken);

        return Result.Success();
    }
}
