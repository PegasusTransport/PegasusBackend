using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class CarRepo(AppDBContext context, ILogger<CarRepo> logger) : ICarRepo
    {
        public async Task<Cars?> FindCarByRegNumberAsync(string licensePlate)
        {
            try
            {
                var car = await context.Cars.FirstOrDefaultAsync(r => r.LicensePlate == licensePlate);

                if (car == null)
                {
                    return null;
                }
                return car;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get car");
                return null;
            }

        }
        public async Task<bool> SaveCar(Cars car)
        {
            try
            {
                if (car == null)
                {
                    logger.LogError("Failed to savecar");
                    return false;
                }
                context.Cars.Add(car);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save car");
                return false;
            }
        }
        public async Task<bool> UpdateCar(Cars car)
        {
            try
            {
                context.Cars.Update(car);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update car");
                return false;
            }
        }
    }
}
