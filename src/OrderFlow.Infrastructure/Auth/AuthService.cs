using Microsoft.AspNetCore.Identity;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return Result.Failure<AuthResponse>(
                AuthErrors.EmailAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return Result.Failure<AuthResponse>(
                AuthErrors.UserCreationFailed);
        }

        await _userManager.AddToRoleAsync(user, Roles.Customer);
        await _customerRepository.AddAsync(Customer.Create(user.Id, request.PhoneNumber, request.Address), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email,
            roles);

        var response = new AuthResponse(
            token,
            DateTime.UtcNow.AddMinutes(60),
            user.Id,
            user.Email,
            roles);

        return Result.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(
            request.Email);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordValid)
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        var customer = await _customerRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        if (customer is not null && !customer.IsActive)
        {
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email!,
            roles);

        var response = new AuthResponse(
            token,
            DateTime.UtcNow.AddMinutes(60),
            user.Id,
            user.Email!,
            roles);

        return Result.Success(response);
    }
}