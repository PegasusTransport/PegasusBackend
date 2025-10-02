using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, TaxiSettings? taxiSettings, string Massage)> GetTaxiPricesAsync();
        Task<(bool Success, TaxiSettings? taxiSettings, string Massage)> CreatePricesAsync(TaxiSettings newSettings);

    }
}
