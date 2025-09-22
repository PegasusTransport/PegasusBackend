using RestrurantPG.Repositories.Implementations;
using RestrurantPG.Repositories.Interfaces;
using RestrurantPG.Services.Implementations;
using RestrurantPG.Services.Interfaces;

namespace RestrurantPG.Configurations
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminRepository, AdminRepository>();

            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<ITableService, TableService>();

            services.AddScoped<IGuestRepository, GuestRepository>();
            services.AddScoped<IGuestService, GuestService>();

            services.AddScoped<IDishRepository, DishRepository>();
            services.AddScoped<IDishService, DishService>();

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();

            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IAdminService, AdminService>();

            return services;
        }
    }
}
