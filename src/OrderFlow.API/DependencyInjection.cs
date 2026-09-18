using Microsoft.AspNetCore.DataProtection;
using OrderFlow.API.ExceptionHandling;
using OrderFlow.Infrastructure.Persistence;
using Serilog;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace OrderFlow.API;

public static class DependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();
        });

        // Persist Data Protection keys so app-pool recycles on shared hosting
        // do not invalidate Identity tokens. Skipped in Testing (ephemeral is fine).
        if (!string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase))
        {
            var keyPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "keys");
            Directory.CreateDirectory(keyPath);
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
                .SetApplicationName("OrderFlow");
        }

        // Optional browser-client support: only registered when origins are configured.
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();
        if (allowedOrigins.Length > 0)
        {
            services.AddCors(options => options.AddPolicy(CorsPolicies.Frontend, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()));
        }

        services.AddHttpContextAccessor();
        services.AddScoped<Application.Common.Identity.ICurrentUser, Abstractions.CurrentUser>();

        services.AddControllers();
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services.AddOpenApi();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    context.HttpContext.Request.Path;

                context.ProblemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;
            };
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: ["ready"]);

        return services;
    }
}
