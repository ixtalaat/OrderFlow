using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Customers;
using System.Security.Claims;
using OrderFlow.Application.Products.DTOs;
using OrderFlow.Application.Products.Queries.GetCatalogProductById;
using OrderFlow.Application.Products.Queries.GetCatalogProducts;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/catalog/products")]
[Authorize(Roles = Roles.Customer)]
public sealed class ProductCatalogController(ISender sender, ICustomerRepository customers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedList<CatalogProductResponse>>> GetAll([FromQuery] CatalogQueryParams query, CancellationToken ct)
    {
        var customer = await GetCurrentCustomer(ct);
        var result = await sender.Send(new GetCatalogProductsQuery(customer?.Id ?? 0, query), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CatalogProductResponse>> GetById(int id, CancellationToken ct)
    {
        var customer = await GetCurrentCustomer(ct);
        var result = await sender.Send(new GetCatalogProductByIdQuery(customer?.Id ?? 0, id), ct);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    private async Task<OrderFlow.Domain.Entities.Customer?> GetCurrentCustomer(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null ? null : await customers.GetByUserIdAsync(userId, ct);
    }
}
