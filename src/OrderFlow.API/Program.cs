using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OrderFlow.API;
using OrderFlow.API.HealthChecks;
using OrderFlow.Infrastructure;
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
    }

    var app = builder.Build();
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseExceptionHandler();

        app.UseHttpsRedirection();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponseAsync
        });

        app.Run();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderFlow API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
