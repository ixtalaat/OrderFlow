using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Inventory.Commands.AddStock;
using OrderFlow.Application.Inventory.Commands.AdjustStock;
using OrderFlow.Application.Inventory.Commands.ConfirmStock;
using OrderFlow.Application.Inventory.Commands.ReleaseStock;
using OrderFlow.Application.Inventory.Commands.ReserveStock;
using OrderFlow.Application.Inventory.DTOs;
using OrderFlow.Application.Inventory.Queries.GetInventory;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
public sealed class InventoryController(ISender sender) : ControllerBase
{
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<InventoryResponse>> Get(int productId, CancellationToken ct)
    {
        var result = await sender.Send(new GetInventoryQuery(productId), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{productId:int}/add-stock")]
    public Task<IActionResult> Add(int productId, InventoryQuantityRequest request, CancellationToken ct)
        => Send(new AddStockCommand(productId, request.Quantity), ct);

    [HttpPost("{productId:int}/reserve")]
    public Task<IActionResult> Reserve(int productId, InventoryQuantityRequest request, CancellationToken ct)
        => Send(new ReserveStockCommand(productId, request.Quantity), ct);

    [HttpPost("{productId:int}/release")]
    public Task<IActionResult> Release(int productId, InventoryQuantityRequest request, CancellationToken ct)
        => Send(new ReleaseStockCommand(productId, request.Quantity), ct);

    [HttpPost("{productId:int}/confirm")]
    public Task<IActionResult> Confirm(int productId, InventoryQuantityRequest request, CancellationToken ct)
        => Send(new ConfirmStockCommand(productId, request.Quantity), ct);

    [HttpPatch("{productId:int}/adjust")]
    public Task<IActionResult> Adjust(int productId, InventoryQuantityRequest request, CancellationToken ct)
        => Send(new AdjustStockCommand(productId, request.Quantity), ct);

    private async Task<IActionResult> Send<T>(T command, CancellationToken ct) where T : MediatR.IRequest<OrderFlow.Application.Common.Results.Result<InventoryResponse>>
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
