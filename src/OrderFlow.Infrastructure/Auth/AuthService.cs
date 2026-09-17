using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Auth;

namespace OrderFlow.Application.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result.Failure<AuthResponse>(AuthErrors.UserCreationFailed);

        try
        {
            var roleResult = await userManager.AddToRoleAsync(user, Roles.Customer);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result.Failure<AuthResponse>(AuthErrors.UserCreationFailed);
            }

            await customerRepository.AddAsync(Customer.Create(user.Id, request.PhoneNumber, request.Address), cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            return Result.Failure<AuthResponse>(AuthErrors.UserCreationFailed);
        }

        var roles = await userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);
        return Result.Success(new AuthResponse(
            jwtTokenService.GenerateToken(user.Id, user.Email!, roles),
            expiresAt,
            user.Id,
            user.Email!,
            roles));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded || passwordResult.IsLockedOut)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var customer = await customerRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (customer is not null && !customer.IsActive)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var roles = await userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);
        return Result.Success(new AuthResponse(
            jwtTokenService.GenerateToken(user.Id, user.Email!, roles),
            expiresAt,
            user.Id,
            user.Email!,
            roles));
    }
}
