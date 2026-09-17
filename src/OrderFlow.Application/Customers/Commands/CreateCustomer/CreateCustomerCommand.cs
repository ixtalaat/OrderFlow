using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers.DTOs;

namespace OrderFlow.Application.Customers.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(string FullName, string Email, string Password, string PhoneNumber, string Address) : IRequest<Result<CustomerResponse>>;
