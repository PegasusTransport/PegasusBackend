using PegasusBackend.DTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepo _adminRepo;
        private readonly ILogger<AdminService> _logger;
        public AdminService(IAdminRepo adminRepo, ILogger<AdminService> logger)
        {
            _adminRepo = adminRepo;
            _logger = logger;
        }

        public async Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync()
        {
            var taxiSettings = await _adminRepo.GetTaxiPricesAsync();

            if (taxiSettings is null)
                return ServiceResponse<TaxiSettings>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Inga prislistor hittades i databasen"
                );

            return ServiceResponse<TaxiSettings>.SuccessResponse(
                HttpStatusCode.OK, 
                taxiSettings,
                "Hämtade den senaste prislistan."
            );
        }

        public async Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO)
        {
            try
            {
                var newSettings = new TaxiSettings
                {
                    ZonePrice = taxiSettingsDTO.ZonePrice,
                    KmPrice = taxiSettingsDTO.KmPrice,
                    StartPrice = taxiSettingsDTO.StartPrice,
                    MinutePrice = taxiSettingsDTO.MinutePrice,
                    UpdatedAt = DateTime.UtcNow
                };

                await _adminRepo.CreateTaxiPricesAsync(newSettings);

                return ServiceResponse<TaxiSettings>.SuccessResponse(
                    HttpStatusCode.OK,
                    newSettings,
                    "Prislistan har uppdaterats."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new taxi prices");
                return ServiceResponse<TaxiSettings>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong");
            }
        }
    }
}
