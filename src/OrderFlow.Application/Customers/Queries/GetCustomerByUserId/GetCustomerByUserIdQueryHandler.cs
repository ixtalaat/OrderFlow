using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomerByUserId;

public sealed class GetCustomerByUserIdQueryHandler : IRequestHandler<GetCustomerByUserIdQuery, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByUserIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByUserIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetResponseByUserIdAsync(query.UserId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        return Result.Success(customer);
    }
}
