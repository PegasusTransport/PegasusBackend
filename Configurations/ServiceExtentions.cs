using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PegasusBackend.Data;
using PegasusBackend.Models;


namespace PegasusBackend.Configurations
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>() // Use User and IdentityRole
              .AddEntityFrameworkStores<AppDBContext> ()
              .AddDefaultTokenProviders(); // Add token providers for password reset, email confirmation, etc.


            services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedEmail = true);

            return services;
        }
    }
}
