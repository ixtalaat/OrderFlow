using MediatR;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Pricing.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.Commands.CreatePricingRule;

public sealed record CreatePricingRuleCommand(int ProductId, CustomerTier Tier, decimal DiscountPercentage, DateTime ValidFromUtc, DateTime? ValidToUtc) : IRequest<Result<PricingRuleResponse>>;
