using MediatR;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    int Id,
    string FullName,
    string PhoneNumber,
    string Address) : IRequest<Result<CustomerResponse>>;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> Handle(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        customer.UpdateDetails(command.PhoneNumber, command.Address);

        var userUpdateResult = await _identityService.UpdateUserFullNameAsync(customer.UserId, command.FullName, cancellationToken);
        if (userUpdateResult.IsFailure)
        {
            return Result.Failure<CustomerResponse>(userUpdateResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await _customerRepository.GetResponseByIdAsync(customer.Id, cancellationToken);
        if (response is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        return Result.Success(response);
    }
}
