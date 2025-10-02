using PegasusBackend.DTOs;
using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, TaxiSettings? TaxiSettings, string Massage)> GetTaxiPricesAsync();
        Task<(bool Success, TaxiSettings? TaxiSettings, string Massage)> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO);

    }
}
