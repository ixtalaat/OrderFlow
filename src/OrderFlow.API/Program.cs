using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OrderFlow.API;
using OrderFlow.API.BackgroundProcessing;
using OrderFlow.API.HealthChecks;
using OrderFlow.API.Observability;
using OrderFlow.Application;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure.Identity;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting OrderFlow API");
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.Services.AddAPI(builder.Configuration);
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication();
    }



    var app = builder.Build();
    {
        using (var scope = app.Services.CreateScope())
        {
            await IdentitySeeder.SeedAsync(scope.ServiceProvider);
        }

        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseSerilogRequestLogging();

        if (app.Services.GetService<IBackgroundJobClient>() is not null)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthorizationFilter()]
            });
        }

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();
        app.UseRateLimiter();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        app.Run();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderFlow API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
