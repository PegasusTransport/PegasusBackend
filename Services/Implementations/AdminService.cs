using PegasusBackend.DTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
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

        public async Task<(bool Success, TaxiSettings? TaxiSettings, string Massage)> GetTaxiPricesAsync()
        {
            var taxiSettings = await _adminRepo.GetTaxiPricesAsync();

            if (taxiSettings is null)
            {
                return (false, null, "Inga prislistor hittades i databasen");
            }

            return (true, taxiSettings, "Hämtade den senaste prislistan.");
        }

        public async Task<(bool Success, TaxiSettings? TaxiSettings, string Massage)> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO)
        {
            if (taxiSettingsDTO == null)
            {
                return (false, null, "Inga nya priser skickades in.");
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
                return (false, null, "Kunde inte uppdatera prislistan.");
            }

            return (true, newTaxiSettings, "Prislistan har uppdaterats.");
        }

    }
}
