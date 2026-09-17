using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomerByUserId;

public sealed record GetCustomerByUserIdQuery(string UserId) : IRequest<Result<CustomerResponse>>;
