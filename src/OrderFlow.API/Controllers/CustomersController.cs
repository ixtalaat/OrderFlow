using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Models;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Customers.Commands.ActivateCustomer;
using OrderFlow.Application.Customers.Commands.CreateCustomer;
using OrderFlow.Application.Customers.Commands.ChangeCustomerTier;
using OrderFlow.Domain.Entities;
using OrderFlow.Application.Customers.Commands.DeactivateCustomer;
using OrderFlow.Application.Customers.Commands.UpdateCustomer;
using OrderFlow.Application.Customers.DTOs;
using OrderFlow.Application.Customers.Queries.GetCustomerById;
using OrderFlow.Application.Customers.Queries.GetCustomerByUserId;
using OrderFlow.Application.Customers.Queries.GetCustomers;
using System.Security.Claims;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FullName,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.Address);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<ActionResult<PagedList<CustomerResponse>>> GetAll(
        [FromQuery] CustomerQueryParams queryParams,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(queryParams);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("me")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<ActionResult<CustomerResponse>> GetCurrentCustomer(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = new GetCustomerByUserIdQuery(userId);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<CustomerResponse>> GetById(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem();
        }

        var customer = result.Value!;

        // If user is only a Customer role, verify resource ownership
        var isSalesOrAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SalesEmployee);
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!isSalesOrAdmin && customer.UserId != currentUserId)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.Forbidden).ToProblem();
        }

        return Ok(customer);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<ActionResult<CustomerResponse>> Update(
        int id,
        [FromBody] UpdateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.FullName,
            request.PhoneNumber,
            request.Address);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPatch("{id:int}/tier")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<IActionResult> ChangeTier(int id, CustomerTier tier, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeCustomerTierCommand(id, tier), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<IActionResult> Activate(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ActivateCustomerCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.SalesEmployee}")]
    public async Task<IActionResult> Deactivate(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateCustomerCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
