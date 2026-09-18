using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Coupons.Commands.CreateCoupon;
using OrderFlow.Application.Coupons.Commands.DeleteCoupon;
using OrderFlow.Application.Coupons.Commands.UpdateCoupon;
using OrderFlow.Application.Coupons.DTOs;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/coupons")]
[Authorize(Roles = Roles.Admin)]
public sealed class CouponsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CouponResponse>> Create(CouponRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateCouponCommand(request.Code, request.DiscountPercentage, request.MinOrderTotal, request.ValidFromUtc, request.ValidToUtc, request.MaxRedemptions), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CouponResponse>> Update(int id, CouponRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCouponCommand(id, request.DiscountPercentage, request.MinOrderTotal, request.ValidFromUtc, request.ValidToUtc, request.MaxRedemptions), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteCouponCommand(id), ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
