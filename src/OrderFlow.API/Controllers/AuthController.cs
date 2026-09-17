using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrderFlow.API.Abstractions;
using OrderFlow.Application.Auth;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Constants;
using System.Security.Claims;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _authService.RegisterAsync(
                request,
                cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
    LoginRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var email = User.FindFirstValue(
            ClaimTypes.Email);

        var roles = User.FindAll(
            ClaimTypes.Role)
            .Select(x => x.Value)
            .ToList();

        return Ok(new
        {
            Id = userId,
            Email = email,
            Roles = roles
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            Message = "You are an admin."
        });
    }
}