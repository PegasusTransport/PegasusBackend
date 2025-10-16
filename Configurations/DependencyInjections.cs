using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Implementations.BookingServices;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;

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
                configuration.GetSection("MailJetSettings")
            );

            // Repositories
            services.AddScoped<IAdminRepo, AdminRepo>();
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IBookingRepo, BookingRepo>();
            services.AddScoped<IDriverRepo, DriverRepo>();

            // Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IMailjetEmailService, MailjetEmailService>();
            services.AddScoped<IBookingService, BookingService>();

            // Booking-related services
            services.AddScoped<IBookingValidationService, BookingValidationService>();
            services.AddScoped<IBookingFactoryService, BookingFactoryService>();
            services.AddScoped<IBookingMapperService, BookingMapperService>();

            // Email Configuration & Service
            var emailConfig = configuration.GetSection("EmailConfig").Get<EmailConfig>();
            services.AddSingleton(emailConfig!);
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}