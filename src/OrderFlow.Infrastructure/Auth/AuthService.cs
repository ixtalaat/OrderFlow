using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Notifications;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Auth;

namespace OrderFlow.Application.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwtTokenService,
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokens,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthOptions> authOptions,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<RegisterResponse>(AuthErrors.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result.Failure<RegisterResponse>(AuthErrors.UserCreationFailed);

        try
        {
            var roleResult = await userManager.AddToRoleAsync(user, Roles.Customer);
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result.Failure<RegisterResponse>(AuthErrors.UserCreationFailed);
            }

            await customerRepository.AddAsync(Customer.Create(user.Id, request.PhoneNumber, request.Address), cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            return Result.Failure<RegisterResponse>(AuthErrors.UserCreationFailed);
        }

        if (authOptions.Value.RequireConfirmedEmail)
            await SendConfirmationEmailAsync(user, cancellationToken);

        return Result.Success(new RegisterResponse(user.Id, user.Email!, authOptions.Value.RequireConfirmedEmail));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded || passwordResult.IsLockedOut)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        if (authOptions.Value.RequireConfirmedEmail && !user.EmailConfirmed)
            return Result.Failure<AuthResponse>(AuthErrors.EmailNotConfirmed);

        var customer = await customerRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (customer is not null && !customer.IsActive)
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        var roles = await userManager.GetRolesAsync(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);
        var refresh = await IssueRefreshTokenAsync(user, cancellationToken);
        return Result.Success(new AuthResponse(
            jwtTokenService.GenerateToken(user.Id, user.Email!, roles, user.TokenVersion),
            expiresAt,
            user.Id,
            user.Email!,
            roles,
            refresh.Token,
            refresh.ExpiresAtUtc));
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure(AuthErrors.InvalidConfirmationToken);

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidConfirmationToken);
    }

    public async Task<Result> ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not null && !user.EmailConfirmed && authOptions.Value.RequireConfirmedEmail)
            await SendConfirmationEmailAsync(user, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<RefreshResponse>> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var stored = await refreshTokens.GetByHashAsync(Hash(request.RefreshToken), cancellationToken);
        if (stored is null)
            return Result.Failure<RefreshResponse>(AuthErrors.InvalidRefreshToken);

        if (!stored.IsActive(now))
        {
            if (stored.ReplacedByTokenHash is not null)
                await RevokeTokenFamilyAsync(stored.UserId, now, cancellationToken);

            return Result.Failure<RefreshResponse>(AuthErrors.InvalidRefreshToken);
        }

        var user = await userManager.FindByIdAsync(stored.UserId);
        if (user is null)
            return Result.Failure<RefreshResponse>(AuthErrors.InvalidRefreshToken);

        var refresh = await IssueRefreshTokenAsync(user, cancellationToken);
        stored.MarkReplaced(refresh.Hash);
        await refreshTokens.RemoveExpiredAsync(now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        return Result.Success(new RefreshResponse(
            jwtTokenService.GenerateToken(user.Id, user.Email!, roles, user.TokenVersion),
            DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes),
            refresh.Token,
            refresh.ExpiresAtUtc));
    }

    public async Task<Result> RevokeAsync(RevokeRequest request, CancellationToken cancellationToken)
    {
        var stored = await refreshTokens.GetByHashAsync(Hash(request.RefreshToken), cancellationToken);
        if (stored is not null && stored.RevokedAtUtc is null)
        {
            stored.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    private async Task RevokeTokenFamilyAsync(string userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        foreach (var sibling in await refreshTokens.ListActiveByUserIdAsync(userId, utcNow, cancellationToken))
            sibling.Revoke();

        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            user.TokenVersion++;
            await userManager.UpdateAsync(user);
        }

        await refreshTokens.RemoveExpiredAsync(utcNow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Refresh token reuse detected for user {UserId}; token family revoked.", userId);
    }

    private async Task<(string Token, string Hash, DateTime ExpiresAtUtc)> IssueRefreshTokenAsync(
        ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var hash = Hash(token);
        var expiresAt = DateTime.UtcNow.AddDays(authOptions.Value.RefreshTokenDays);
        await refreshTokens.AddAsync(RefreshToken.Create(user.Id, hash, expiresAt), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (token, hash, expiresAt);
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await emailSender.SendAsync(
                user.Email!,
                "Confirm your OrderFlow email",
                $"Confirm your email address with this token: {token}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Confirmation email could not be sent to user {UserId}.", user.Id);
        }
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
