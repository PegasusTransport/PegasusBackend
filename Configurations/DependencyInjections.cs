using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Configurations
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Alla DIs ska in hit!
            services.AddScoped<IAdminRepo, AdminRepo>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUserService,  UserService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IAuthService, AuthService>();


            return services;
        }
    }
}
