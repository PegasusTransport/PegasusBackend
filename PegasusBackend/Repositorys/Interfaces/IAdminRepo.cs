using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IAdminRepo
    {
        Task<TaxiSettings?> GetTaxiPricesAsync();
        Task CreateTaxiPricesAsync(TaxiSettings updatedSettings);
    }
}
