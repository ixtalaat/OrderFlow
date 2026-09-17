using MediatR;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        IIdentityService identityService,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var isEmailUnique = await _identityService.IsEmailUniqueAsync(command.Email, cancellationToken);
        if (!isEmailUnique)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.EmailAlreadyExists);
        }

        var userResult = await _identityService.CreateUserAsync(
            command.Email,
            command.Password,
            command.FullName,
            Roles.Customer,
            cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<CustomerResponse>(userResult.Error);
        }

        var (userId, email, fullName) = userResult.Value!;

        var customer = Customer.Create(userId, command.PhoneNumber, command.Address);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CustomerResponse(
            customer.Id,
            customer.UserId,
            fullName,
            email,
            customer.PhoneNumber,
            customer.Address,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);

        return Result.Success(response);
    }
}
