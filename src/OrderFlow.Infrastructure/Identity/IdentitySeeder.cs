using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.Application.Common.Constants;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        var db =
            services.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        var adminOptions =
            services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        await SeedRolesAsync(roleManager);

        await SeedAdminAsync(
            userManager,
            adminOptions);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            Roles.Admin,
            Roles.SalesEmployee,
            Roles.Customer
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            var result = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{role}': " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        AdminSeedOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Email))
            throw new InvalidOperationException(
                "Admin seed email is not configured.");

        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException(
                "Admin seed password is not configured.");

        var admin = await userManager.FindByEmailAsync(options.Email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                FullName = options.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                admin,
                options.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create admin user: " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(
                admin,
                Roles.Admin))
        {
            var result = await userManager.AddToRoleAsync(
                admin,
                Roles.Admin);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to assign Admin role: " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }
}
