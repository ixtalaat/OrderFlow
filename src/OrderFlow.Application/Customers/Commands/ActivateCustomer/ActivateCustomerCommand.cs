using MediatR;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.ActivateCustomer;

public sealed record ActivateCustomerCommand(int Id) : IRequest<Result>;

public sealed class ActivateCustomerCommandHandler : IRequestHandler<ActivateCustomerCommand, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
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

        return Result.Success();
    }
}
