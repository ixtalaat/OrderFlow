using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Pricing.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Commands.UpdatePricingRule;

public sealed record UpdatePricingRuleCommand(int Id, CustomerTier Tier, decimal DiscountPercentage, DateTime ValidFromUtc, DateTime? ValidToUtc) : IRequest<Result<PricingRuleResponse>>;
