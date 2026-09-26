using Microsoft.AspNetCore.DataProtection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlow.API.ExceptionHandling;
using OrderFlow.API.Observability;
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
            options.AddPolicy("auth", context => ForIp(context, configuration.GetValue("RateLimiting:Auth:PermitLimit", 10)));
            options.AddPolicy("catalog", context => ForIp(context, configuration.GetValue("RateLimiting:Catalog:PermitLimit", 100)));
            options.AddPolicy("orders", context => ForIp(context, configuration.GetValue("RateLimiting:Orders:PermitLimit", 30)));
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

        var telemetry = configuration.GetSection(TelemetryOptions.SectionName).Get<TelemetryOptions>() ?? new TelemetryOptions();
        var testing = string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Testing", StringComparison.OrdinalIgnoreCase);
        if (telemetry.Enabled && !testing)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(OrderFlowTelemetry.ServiceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource(OrderFlowTelemetry.ServiceName);
                    if (!string.IsNullOrWhiteSpace(telemetry.OtlpEndpoint))
                        tracing.AddOtlpExporter(options => options.Endpoint = new Uri(telemetry.OtlpEndpoint));
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter(OrderFlowTelemetry.ServiceName);
                    if (!string.IsNullOrWhiteSpace(telemetry.OtlpEndpoint))
                        metrics.AddOtlpExporter(options => options.Endpoint = new Uri(telemetry.OtlpEndpoint));
                });
        }

        return services;
    }

    private static RateLimitPartition<string> ForIp(HttpContext context, int permitLimit)
        => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
}
