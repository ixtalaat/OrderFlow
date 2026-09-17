using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetResponseByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        return Result.Success(customer);
    }
}
