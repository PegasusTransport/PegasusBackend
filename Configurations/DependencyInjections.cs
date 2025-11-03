using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Implementations.BookingServices;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using PegasusBackend.Validators.MailjetValidators;
using QuestPDF.Infrastructure;

namespace PegasusBackend.Configurations
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Other Configs
            services.AddHttpClient();
            services.AddQuestPdfConfiguration();

            // Mailjet settings correct
            services.Configure<MailJetSettings>(
                configuration.GetSection("Mailjet")
            );

            //PaginationService
            services.Configure<PaginationSettings>(configuration.GetSection("Pagination"));

            //bookingRule and helpers
            services.Configure<BookingRulesSettings>(configuration.GetSection("BookingRules"));
            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<BookingRulesSettings>>().Value);

            services.AddScoped<RecalculateIfAddressChangedHelper>();
            services.AddScoped<ValidateUpdateRuleHelper>();

            // Register all FluentValidators
            services.AddValidatorsFromAssemblyContaining<AccountWelcomeRequestValidator>();

            // Repositories
            services.AddScoped<IAdminRepo, AdminRepo>();
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IBookingRepo, BookingRepo>();
            services.AddScoped<IDriverRepo, DriverRepo>();
            services.AddScoped<IIdempotencyRepo, IdempotencyRepo>();

            // Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IMailjetEmailService, MailjetEmailService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();

            // Booking-related services
            services.AddScoped<IBookingValidationService, BookingValidationService>();
            services.AddScoped<IBookingFactoryService, BookingFactoryService>();
            services.AddScoped<IBookingMapperService, BookingMapperService>();

            // Email Configuration & Service
            var emailConfig = configuration.GetSection("EmailConfig").Get<EmailConfig>();
            services.AddSingleton(emailConfig!);



            return services;
        }
    }
}