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
        public async Task<DriverDTO?> GetDriverByIdAsync(Guid id)
        {
            try
            {
                var driver = await context.Drivers
                   .AsNoTracking() 
                   .Include(d => d.User)
                   .Where(d => d.DriverId == id)
                   .Select(d => new DriverDTO 
                   {
                       Id = d.DriverId,
                       FirstName = d.User.FirstName,
                       LastName = d.User.LastName,
                       ProfilePicture = d.ProfilePicture
                   })
                   .FirstOrDefaultAsync();

                if (driver == null)
                {
                    logger.LogWarning("Driver with ID {DriverId} not found", id);
                }

                return driver;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving driver with ID {DriverId}", id);
                throw; 
            }
        }
        public async Task<DriverDTO?> GetDriverByUserIdAsync(string userId)
        {
            try
            {
                return await context.Drivers
                    .AsNoTracking()
                    .Include(d => d.User)
                    .Where(d => !d.IsDeleted&& !d.User.IsDeleted)
                    .Select(d => new DriverDTO
                    {
                        Id = d.DriverId,
                        FirstName = d.User.FirstName,
                        LastName = d.User.LastName,
                        ProfilePicture = d.ProfilePicture,
                        CarId = d.CarId
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving driver for user {UserId}", userId);
                return null;
            }
        }
        public async Task<List<AllDriversDTO>> GetAllDrivers()
        {
            try
            {
                var drivers = await context.Drivers
                    .Where(d => !d.IsDeleted && !d.User.IsDeleted)
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
        public async Task<bool> UpdateDriver(UpdateDriverDTO request, Guid driverId)
        {
            try
            {
                var driver = await context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);

                if (driver == null)
                {
                    logger.LogError("Can't find driver");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(request.ProfilePicture))
                    driver.ProfilePicture = request.ProfilePicture;

                if (request.CarId.HasValue)
                    driver.CarId = request.CarId.Value;

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update driver");
                return false;
            }
        }
        public async Task<bool> DeleteDriver(Guid driverId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var driver = await context.Drivers.FirstOrDefaultAsync(d => d.DriverId == driverId);
                var booking = await context.Bookings
                    .Where(b => b.DriverIdFK == driverId && !b.IsConfirmed).ToListAsync();

                SetBookingsToAvailable(booking);

                if (driver == null)
                {
                    logger.LogWarning("Driver {DriverId} not found", driverId);
                    return false;
                }

                driver.IsDeleted = true;
                driver.DeletedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to delete driver {DriverId}", driverId);
                return false;
            }
        }
        private static void SetBookingsToAvailable(List<Bookings> bookings)
        {
            foreach (var booking in bookings)
            {
                booking.DriverIdFK = null;
                booking.IsAvailable = true;
            }
        }
    }
}
