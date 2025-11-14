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
            var car = await context.Cars.FirstOrDefaultAsync(r => r.LicensePlate == licensePlate);

            if (car == null)
            {
                return null;
            }
            return car;

        }
        public async Task<bool> SaveCar(Cars car)
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
    }
}
