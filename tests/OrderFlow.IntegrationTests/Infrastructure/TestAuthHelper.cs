using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Auth;
using OrderFlow.Infrastructure.Identity;

namespace OrderFlow.IntegrationTests.Infrastructure;

public static class TestAuthHelper
{
    public static async Task<(string UserId, string Token)> CreateUserAndGetTokenAsync(
        CustomWebApplicationFactory factory,
        string role,
        string? email = null,
        string? fullName = null)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var jwtTokenService = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();

        var userEmail = email ?? $"{role.ToLower()}-{Guid.NewGuid()}@test.com";
        var userFullName = fullName ?? $"{role} User";

        var user = new ApplicationUser
        {
            UserName = userEmail,
            Email = userEmail,
            FullName = userFullName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "Password@123");
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to create test user: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, role);
        var roles = await userManager.GetRolesAsync(user);

        var token = jwtTokenService.GenerateToken(user.Id, user.Email!, roles);
        return (user.Id, token);
    }
}
