using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Orders.Commands.CancelOrder;
using OrderFlow.Application.Orders.Commands.CompleteOrder;
using OrderFlow.Application.Orders.Commands.ConfirmOrder;
using OrderFlow.Application.Orders.Commands.CreateOrder;
using OrderFlow.Application.Orders.Commands.ProcessOrder;
using OrderFlow.Application.Orders.Commands.RejectOrder;
using OrderFlow.Application.Orders.DTOs;
using OrderFlow.Application.Orders.Queries.GetCustomerOrders;
using OrderFlow.Application.Orders.Queries.GetOrder;
using OrderFlow.Application.Customers;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController(ISender sender, ICustomerRepository customers, IIdempotencyService idempotency) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customer = userId is null ? null : await customers.GetByUserIdAsync(userId, ct);
        if (customer is null) return Unauthorized();
        var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var values) ? values.ToString() : null;
        if (idempotencyKey is not null)
        {
            var replay = await idempotency.LookupAsync(userId!, idempotencyKey, request, ct);
            if (replay is not null)
            {
                if (!replay.PayloadMatches || replay.Order is null)
                    return Result.Failure<OrderResponse>(OrderErrors.IdempotentPayloadMismatch).ToProblem();
                return CreatedAtAction(nameof(Get), new { id = replay.Order.Id }, replay.Order);
            }
        }
        var result = await sender.Send(new CreateOrderCommand(customer.Id, request.Items), ct);
        if (result.IsFailure) return result.ToProblem();
        if (idempotencyKey is not null)
            await idempotency.RecordAsync(userId!, idempotencyKey, request, result.Value!, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> Get(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetOrderQuery(id), ct);
        if (result.IsFailure) return result.ToProblem();
        var isStaff = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SalesEmployee);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customer = userId is null ? null : await customers.GetByUserIdAsync(userId, ct);
        return isStaff || customer?.Id == result.Value!.CustomerId ? Ok(result.Value) : Forbid();
    }

    [HttpGet]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<PagedList<OrderResponse>>> GetMine([FromQuery] OrderQueryParams query, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customer = userId is null ? null : await customers.GetByUserIdAsync(userId, ct);
        if (customer is null) return Unauthorized();
        var result = await sender.Send(new GetCustomerOrdersQuery(customer.Id, query), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var customer = userId is null ? null : await customers.GetByUserIdAsync(userId, ct);
        if (customer is null) return Unauthorized();
        var result = await sender.Send(new CancelOrderCommand(id, customer.Id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("{id:int}/confirm")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public Task<IActionResult> Confirm(int id, CancellationToken ct) => Status(new ConfirmOrderCommand(id), ct);
    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public Task<IActionResult> Reject(int id, CancellationToken ct) => Status(new RejectOrderCommand(id), ct);
    [HttpPatch("{id:int}/processing")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public Task<IActionResult> Process(int id, CancellationToken ct) => Status(new ProcessOrderCommand(id), ct);
    [HttpPatch("{id:int}/complete")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public Task<IActionResult> Complete(int id, CancellationToken ct) => Status(new CompleteOrderCommand(id), ct);

    private async Task<IActionResult> Status<T>(T command, CancellationToken ct) where T : IRequest<OrderFlow.Application.Common.Results.Result<OrderResponse>>
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
