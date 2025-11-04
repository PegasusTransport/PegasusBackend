using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PegasusBackend.Data;
using PegasusBackend.Filters;
using PegasusBackend.Models;
using PegasusBackend.Services.BackgroundServices;


namespace PegasusBackend.Configurations
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>() // Use User and IdentityRole
              .AddEntityFrameworkStores<AppDBContext>()
              .AddDefaultTokenProviders();

            var passwordResetTokenLifetimeHours = config
             .GetSection("PasswordResetSettings")
             .GetValue<int>("TokenLifetimeHours");

            services.Configure<PasswordResetTokenProviderOptions>(options =>
              options.TokenLifespan = TimeSpan.FromHours(passwordResetTokenLifetimeHours));

            services.Configure<DataProtectionTokenProviderOptions>(options =>
              options.TokenLifespan = TimeSpan.FromHours(24));

            services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedEmail = true);

            return services;
        }

        public static IServiceCollection AddIdempotencyServices(
            this IServiceCollection services,
             IConfiguration configuration)
        {
            // Configure IdempotencySettings from appsettings.json
            services.Configure<IdempotencySettings>(
                configuration.GetSection("IdempotencySettings")
            );

            // Register background cleanup service
            services.AddHostedService<IdempotencyCleanupService>();

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(
            this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // Add Idempotency-Key header to Swagger UI
                c.OperationFilter<IdempotencyHeaderFilter>();
            });

            return services;
        }
    }
}
