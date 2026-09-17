using Microsoft.AspNetCore.Http;
using OrderFlow.Application.Common.Results;

namespace OrderFlow.Application.Pricing;

public static class PricingErrors
{
    public static readonly Error NotFound = new("PricingRule.NotFound", "Pricing rule not found.", StatusCodes.Status404NotFound);
    public static readonly Error Overlap = new("PricingRule.Overlap", "An overlapping pricing rule already exists.", StatusCodes.Status409Conflict);
    public static readonly Error InvalidValidity = new("PricingRule.InvalidValidity", "Pricing rule validity is invalid.", StatusCodes.Status400BadRequest);
}
