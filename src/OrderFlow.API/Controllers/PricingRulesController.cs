using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Pricing.Commands.CreatePricingRule;
using OrderFlow.Application.Pricing.Commands.DeletePricingRule;
using OrderFlow.Application.Pricing.Commands.UpdatePricingRule;
using OrderFlow.Application.Pricing.DTOs;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/pricing-rules")]
[Authorize(Roles = Roles.Admin)]
public sealed class PricingRulesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PricingRuleResponse>> Create(PricingRuleRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePricingRuleCommand(request.ProductId, request.Tier, request.DiscountPercentage, request.ValidFromUtc, request.ValidToUtc), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PricingRuleResponse>> Update(int id, PricingRuleRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdatePricingRuleCommand(id, request.Tier, request.DiscountPercentage, request.ValidFromUtc, request.ValidToUtc), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeletePricingRuleCommand(id), ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
