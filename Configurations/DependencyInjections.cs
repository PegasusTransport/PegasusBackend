using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Configurations
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Mailjet settings korrekt
            services.Configure<MailJetSettings>(
                configuration.GetSection("Mailjet")
            );

            // Repositories
            services.AddScoped<IAdminRepo, AdminRepo>();
            services.AddScoped<IUserRepo, UserRepo>();

            // Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IMailjetEmailService, MailjetEmailService>();

            // Email Configuration & Service
            var emailConfig = configuration.GetSection("EmailConfig").Get<EmailConfig>();
            services.AddSingleton(emailConfig!);
            services.AddScoped<IEmailService, EmailService>();


            return services;
        }
    }
}