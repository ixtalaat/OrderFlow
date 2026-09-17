using MediatR;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Pricing.Commands.DeletePricingRule;

public sealed record DeletePricingRuleCommand(int Id) : IRequest<Result>;
