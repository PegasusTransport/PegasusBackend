using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class AdminRepo : IAdminRepo
    {
        private readonly AppDBContext _dbContext;

        public AdminRepo(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task<TaxiSettings?> GetTaxiPricesAsync()
        {
            var taxiSettings = await _dbContext.TaxiSettings
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync();

            return taxiSettings;
        }

        public async Task<TaxiSettings?> CreateTaxiPricesAsync(TaxiSettings newSettings)
        {
            _dbContext.TaxiSettings.Add(newSettings);
            await _dbContext.SaveChangesAsync();
            return newSettings;
        }

    }
}
