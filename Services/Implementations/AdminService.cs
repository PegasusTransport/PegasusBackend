using PegasusBackend.DTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepo _adminRepo;

        public AdminService(IAdminRepo adminRepo)
        {
            _adminRepo = adminRepo;
        }

        public async Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync()
        {
            var taxiSettings = await _adminRepo.GetTaxiPricesAsync();

            if (taxiSettings is null)
            {
                return ServiceResponse<TaxiSettings>.FailResponse(
                    "Inga prislistor hittades i databasen"
                );
            }

            return ServiceResponse<TaxiSettings>.SuccessResponse(
                taxiSettings,
                "Hämtade den senaste prislistan."
            );
        }

        public async Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO)
        {
            if (taxiSettingsDTO == null)
            {
                return ServiceResponse<TaxiSettings>.FailResponse(
                    "Inga nya priser skickades in."
                );
            }

            var newPrices = new TaxiSettings
            {
                ZonePrice = taxiSettingsDTO.ZonePrice,
                KmPrice = taxiSettingsDTO.KmPrice,
                StartPrice = taxiSettingsDTO.StartPrice,
                MinutePrice = taxiSettingsDTO.MinutePrice,
                UpdatedAt = DateTime.UtcNow
            };

            var newTaxiSettings = await _adminRepo.CreateTaxiPricesAsync(newPrices);

            if (newTaxiSettings == null)
            {
                return ServiceResponse<TaxiSettings>.FailResponse(
                    "Kunde inte uppdatera prislistan."
                );
            }

            return ServiceResponse<TaxiSettings>.SuccessResponse(
                newTaxiSettings,
                "Prislistan har uppdaterats."
            );
        }
    }
}
