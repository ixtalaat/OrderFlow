using Microsoft.AspNetCore.Identity;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Results;
using OrderFlow.Application.Customers;

namespace OrderFlow.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<(string UserId, string Email, string FullName)>> CreateUserAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return Result.Failure<(string UserId, string Email, string FullName)>(CustomerErrors.EmailAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            // Staff-provisioned accounts are trusted: they skip email confirmation.
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Result.Failure<(string UserId, string Email, string FullName)>(CustomerErrors.UserCreationFailed);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Failure<(string UserId, string Email, string FullName)>(CustomerErrors.UserCreationFailed);
        }

        return Result.Success((user.Id, user.Email!, user.FullName));
    }

    public async Task<Result> UpdateUserFullNameAsync(
        string userId,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(CustomerErrors.UserNotFound);
        }

        user.FullName = fullName;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure(CustomerErrors.UserCreationFailed);
        }

        return Result.Success();
    }

    public async Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null;
    }

    public async Task<bool> UserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null;
    }

    public async Task IncrementTokenVersionAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return;

        user.TokenVersion++;
        await _userManager.UpdateAsync(user);
    }
}
