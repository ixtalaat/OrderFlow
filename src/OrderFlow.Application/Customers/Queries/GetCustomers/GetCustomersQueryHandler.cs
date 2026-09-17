using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PagedList<CustomerResponse>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<PagedList<CustomerResponse>>> Handle(
        GetCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagedList = await _customerRepository.GetPagedListAsync(query.QueryParams, cancellationToken);
        return Result.Success(pagedList);
    }
}
