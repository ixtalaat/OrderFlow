using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Pricing.DTOs;

public sealed record PricingRuleRequest(int ProductId, CustomerTier Tier, decimal DiscountPercentage, DateTime ValidFromUtc, DateTime? ValidToUtc);
