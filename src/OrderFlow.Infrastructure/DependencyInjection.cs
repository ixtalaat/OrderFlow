using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Auth;
using OrderFlow.Application.BackgroundProcessing;
using OrderFlow.Application.Common.Identity;
using OrderFlow.Application.Common.Persistence;
using OrderFlow.Application.Customers;
using OrderFlow.Application.Inventory;
using OrderFlow.Application.Orders;
using OrderFlow.Application.Notifications;
using OrderFlow.Application.Pricing;
using OrderFlow.Application.Pricing.Strategies;
using OrderFlow.Application.Products;
using OrderFlow.Infrastructure.Auth;
using OrderFlow.Infrastructure.BackgroundProcessing;
using OrderFlow.Infrastructure.Customers;
using OrderFlow.Infrastructure.Identity;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Orders;
using OrderFlow.Infrastructure.Pricing;
using OrderFlow.Infrastructure.Products;
using OrderFlow.Infrastructure.Notifications;
using System.Text;


namespace OrderFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var production = string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase);
        if (production && (!bool.TryParse(configuration["Email:Enabled"], out var emailEnabled) || !emailEnabled))
            throw new InvalidOperationException("Email:Enabled must be true in production.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        if (connectionString?.StartsWith("DataSource=", StringComparison.OrdinalIgnoreCase) == true)
        {
            services.AddSingleton<IBackgroundJobScheduler, NoOpBackgroundJobScheduler>();
        }
        else
        {
            services.AddHangfire(config => config.UseSqlServerStorage(connectionString));
            services.AddHangfireServer(options => options.WorkerCount = Math.Max(1, Environment.ProcessorCount / 2));
            services.AddScoped<IBackgroundJobScheduler, HangfireJobScheduler>();
            services.AddHostedService<OutboxDispatcher>();
        }

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
        });

        services.Configure<AdminSeedOptions>(
            configuration.GetSection(AdminSeedOptions.SectionName));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(x => !x.Enabled || (!string.IsNullOrWhiteSpace(x.Host) && x.Port > 0 && !string.IsNullOrWhiteSpace(x.From) && !string.IsNullOrWhiteSpace(x.UserName) && !string.IsNullOrWhiteSpace(x.Password)), "Enabled email requires Host, Port, From, UserName, and Password.")
            .ValidateOnStart();

        services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtOptions = configuration
                .GetSection(JwtOptions.SectionName)
                .Get<JwtOptions>()
                ?? throw new InvalidOperationException(
                    "JWT configuration is missing.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IOrderNotificationService, OrderNotificationService>();
        services.AddScoped<OrderNotificationTemplate>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPricingRuleRepository, PricingRuleRepository>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<RegularPricingStrategy>();
        services.AddScoped<WholesalePricingStrategy>();
        services.AddScoped<VipPricingStrategy>();
        services.AddScoped<IPricingStrategy>(sp => sp.GetRequiredService<RegularPricingStrategy>());
        services.AddScoped<IPricingStrategy>(sp => sp.GetRequiredService<WholesalePricingStrategy>());
        services.AddScoped<IPricingStrategy>(sp => sp.GetRequiredService<VipPricingStrategy>());
        services.AddScoped<PricingStrategyResolver>();

        return services;
    }
}
