using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Auth;
using OrderFlow.Application.Auth.DTOs;
using OrderFlow.Infrastructure.Identity;

namespace OrderFlow.IntegrationTests.Infrastructure;

public static class TestAuthHelper
{
    public static async Task<(string UserId, string Token)> CreateUserAndGetTokenAsync(
        WebApplicationFactory<Program> factory,
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

        var token = jwtTokenService.GenerateToken(user.Id, user.Email!, roles, user.TokenVersion);
        return (user.Id, token);
    }

    public static async Task ConfirmEmailAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        string email)
    {
        string token;
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email)
                ?? throw new InvalidOperationException($"Test user '{email}' was not found.");
            token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        var response = await client.PostAsJsonAsync(
            "/api/auth/confirm-email",
            new ConfirmEmailRequest(email, token));
        response.EnsureSuccessStatusCode();
    }

    public static async Task<AuthResponse> RegisterConfirmAndLoginAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        string email,
        string password = "Password@123",
        string fullName = "Test Customer")
    {
        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, password, fullName));
        register.EnsureSuccessStatusCode();

        await ConfirmEmailAsync(factory, client, email);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));
        login.EnsureSuccessStatusCode();
        return (await login.Content.ReadFromJsonAsync<AuthResponse>())!;
    }
}
