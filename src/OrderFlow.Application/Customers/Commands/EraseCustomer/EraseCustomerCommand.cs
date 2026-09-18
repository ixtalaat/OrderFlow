using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.EraseCustomer;

public sealed record EraseCustomerCommand(int CustomerId) : IRequest<Result>;
