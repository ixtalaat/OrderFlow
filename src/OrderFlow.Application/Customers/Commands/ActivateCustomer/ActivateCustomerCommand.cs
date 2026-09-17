using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Customers.Commands.ActivateCustomer;

public sealed record ActivateCustomerCommand(int Id) : IRequest<Result>;
