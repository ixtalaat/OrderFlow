using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries.GetAllOrders;
using OrderFlow.Application.Orders.Queries.GetDashboardStats;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsResponse>> Stats(CancellationToken ct)
    {
        var result = await sender.Send(new GetDashboardStatsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<PagedList<OrderResponse>>> Orders([FromQuery] OrderQueryParams query, CancellationToken ct)
    {
        var result = await sender.Send(new GetAllOrdersQuery(query), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
