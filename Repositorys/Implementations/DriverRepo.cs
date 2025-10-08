using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;

namespace PegasusBackend.Repositorys.Implementations
{
    public class DriverRepo(AppDBContext context, ILogger<DriverRepo> logger) : IDriverRepo
    {
        public async Task<bool> CreateDriver(CreateDriverDTO request, string userId)
        {
         
			try
			{
                var existingDriver = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

                if (existingDriver != null)
                {
                    logger.LogWarning("User {UserId} is already a driver", userId);
                    return false;
                }
                var newDriver = new Drivers
                {
                    DriverId = Guid.NewGuid(),
                    UserId = userId,
                    ProfilePicture = request.ProfilePicture
                };
                context.Drivers.Add(newDriver);
                await context.SaveChangesAsync();
                return true;
            }
			catch (Exception ex)
			{
                logger.LogError(ex, $"Failed to create driver for user {userId}");
                return false;
            }
        }

        public async Task<List<AllDriversDTO>> GetAllDrivers()
        {
            try
            {
                var drivers = await context.Drivers
                    .Include(u => u.User)
                    .Select(d => new AllDriversDTO  
                    {
                        FirstName = d.User.FirstName,
                        LastName = d.User.LastName,
                        ProfilePicture = d.ProfilePicture
                    })
                    .ToListAsync();

                return drivers; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Couldn't retrieve all drivers");
                return [];
            }
        }
        public async Task<bool> UpdateDriver(UpdateDriverDTO request, string userId)
        {
            var driver = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                logger.LogError("Cant find driver");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(request.ProfilePicture)) 
                driver.ProfilePicture = request.ProfilePicture;

            if (request.CarId.HasValue)
                driver.CarId = request.CarId.Value;

            return true;
        }
    }
}
