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

        public async Task<(bool Success, TaxiSettings? taxiSettings, string Massage)> GetTaxiPricesAsync()
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();

            if (prices is null)
            {
                return (false, null, "Inga prislistor hittades i databasen");
            }

            return (true, prices, "Hämtade den senaste prislistan.");
        }

        public async Task<(bool Success, TaxiSettings? taxiSettings, string Massage)> CreatePricesAsync(TaxiSettings newSettings)
        {
            if (newSettings == null)
            {
                return (false, null, "Inga nya priser skickades in.");
            }

            var newPrices = await _adminRepo.CreateTaxiPricesAsync(newSettings);

            if (newPrices == null)
            {
                return (false, null, "Kunde inte uppdatera prislistan.");
            }

            return (true, newPrices, "Prislistan har uppdaterats.");
        }

    }
}
