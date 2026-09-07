using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Auth.Validators;

namespace OrderFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }

}
