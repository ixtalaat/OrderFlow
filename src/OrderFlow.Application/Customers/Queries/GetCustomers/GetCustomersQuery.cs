using MediatR;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery(CustomerQueryParams QueryParams) : IRequest<Result<PagedList<CustomerResponse>>>;
