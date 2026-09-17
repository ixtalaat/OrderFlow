using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Customers.Commands.ChangeCustomerTier;

public sealed record ChangeCustomerTierCommand(int CustomerId, CustomerTier Tier) : IRequest<Result>;
