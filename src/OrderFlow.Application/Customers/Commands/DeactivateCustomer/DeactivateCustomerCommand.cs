using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.DeactivateCustomer;

public sealed record DeactivateCustomerCommand(int Id) : IRequest<Result>;
