using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(int Id, string FullName, string PhoneNumber, string Address) : IRequest<Result<CustomerResponse>>;
