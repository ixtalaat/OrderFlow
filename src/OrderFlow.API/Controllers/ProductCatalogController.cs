using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Application.Products.Queries.GetCatalogProductById;
using OrderFlow.Application.Products.Queries.GetCatalogProducts;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
[Authorize(Roles = Roles.Customer)]
public sealed class ProductCatalogController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedList<CatalogProductResponse>>> GetAll([FromQuery] CatalogQueryParams query, CancellationToken ct)
    {
        var result = await sender.Send(new GetCatalogProductsQuery(query), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogProductResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCatalogProductByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
