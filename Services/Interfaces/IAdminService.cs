using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync();
        Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO);

    }
}
