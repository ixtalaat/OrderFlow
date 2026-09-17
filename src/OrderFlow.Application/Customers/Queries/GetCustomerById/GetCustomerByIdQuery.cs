using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(int CustomerId) : IRequest<Result<CustomerResponse>>;
