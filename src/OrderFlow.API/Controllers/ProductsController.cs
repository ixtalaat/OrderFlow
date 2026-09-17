using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Application.Products.Commands.ActivateProduct;
using OrderFlow.Application.Products.Commands.CreateProduct;
using OrderFlow.Application.Products.Commands.DeactivateProduct;
using OrderFlow.Application.Products.Commands.UpdateProduct;
using OrderFlow.Application.Products.Queries.GetProductById;
using OrderFlow.Application.Products.Queries.GetProducts;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee},{Roles.Customer}")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateProductCommand(request.Name, request.Description, request.Sku, request.Price, request.CategoryName), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : result.ToProblem();
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<ProductResponse>>> GetAll([FromQuery] ProductQueryParams query, CancellationToken ct)
    {
        var activeOnly = User.IsInRole(Roles.Customer);
        var result = await sender.Send(new GetProductsQuery(query, activeOnly), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetProductByIdQuery(id, User.IsInRole(Roles.Customer)), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateProductCommand(id, request.Name, request.Description, request.Sku, request.Price, request.CategoryName), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeactivateProductCommand(id), ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        var result = await sender.Send(new ActivateProductCommand(id), ct);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
