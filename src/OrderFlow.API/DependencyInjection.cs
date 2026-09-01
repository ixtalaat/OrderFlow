using OrderFlow.API.ExceptionHandling;
using Serilog;

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

        services.AddControllers();

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

        return services;
    }
}
