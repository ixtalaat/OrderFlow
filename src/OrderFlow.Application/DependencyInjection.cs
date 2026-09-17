using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Customers.Commands.ActivateCustomer;
using OrderFlow.Application.Customers.Commands.CreateCustomer;
using OrderFlow.Application.Customers.Commands.DeactivateCustomer;
using OrderFlow.Application.Customers.Commands.UpdateCustomer;
using OrderFlow.Application.Customers.Queries.GetCustomerById;
using OrderFlow.Application.Customers.Queries.GetCustomerByUserId;
using OrderFlow.Application.Customers.Queries.GetCustomers;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;

namespace OrderFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
