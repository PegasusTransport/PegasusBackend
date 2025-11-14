using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Repositorys.Implementations
{
    public class DriverRepo(AppDBContext context, ILogger<DriverRepo> logger, ICarService _carService) : IDriverRepo
    {
        public async Task<DriverResponseDto?> GetDriverByIdAsync(Guid id)
        {
            try
            {
                var driver = await context.Drivers
                   .AsNoTracking() 
                   .Include(d => d.User)
                   .Where(d => d.DriverId == id)
                   .Select(d => new DriverResponseDto 
                   {
                       DriverId = d.DriverId,
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
        public async Task<Drivers?> GetDriverEntityByIdAsync(Guid driverId)
        {
            return await context.Drivers
                .Include(d => d.Bookings)
                .FirstOrDefaultAsync(d => d.DriverId == driverId);
        }
        public async Task<DriverResponseDto?> GetDriverByUserIdAsync(string userId)
        {
            try
            {
                return await context.Drivers
                    .AsNoTracking()
                    .Include(d => d.Car)
                    .Include(d => d.User)
                    .Where(d => d.UserId == userId && !d.IsDeleted && !d.User.IsDeleted)
                    .Select(d => new DriverResponseDto
                    {
                        DriverId = d.DriverId,
                        FirstName = d.User.FirstName,
                        LastName = d.User.LastName,
                        ProfilePicture = d.ProfilePicture,
                        CarId = d.CarId,
                        CarMake = d.Car.Make,
                        CarModel = d.Car.Model,
                        CarCapacity = d.Car.Capacity,
                        CarType = d.Car.Type,
                        CarLicensePlate = d.Car.LicensePlate
                        
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving driver for user {UserId}", userId);
                return null;
            }
        }
        public async Task<List<Drivers>> GetAllDriversAsync()
        {
            try
            {
                var drivers = await context.Drivers
                    .Include(d => d.User)
                    .Include(d => d.Bookings)
                    .Include(d => d.Car)
                    .Where(d => !d.IsDeleted && !d.User.IsDeleted)
                    .ToListAsync();

                return drivers; 
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Couldn't retrieve all drivers");
                return [];
            }
        }
        public async Task<bool> CreateDriver(CreateRequestDriverDto request)
        {

            try
            {
                var existingDriver = await context.Drivers.FirstOrDefaultAsync(d => d.UserId == request.UserId);

                if (existingDriver != null)
                {
                    logger.LogWarning("User {UserId} is already a driver", request.UserId);
                    return false;
                }
                var car = await _carService.CreateOrFindCar(request.LicensePlate);

                if (car == null)
                {
                    logger.LogWarning("Car null");
                    return false;
                }
                var newDriver = new Drivers
                {
                    DriverId = Guid.NewGuid(),
                    UserId = request.UserId,
                    ProfilePicture = request.ProfilePicture,
                    CarId = car.CarId
                };
                context.Drivers.Add(newDriver);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to create driver for user {request.UserId}");
                return false;       
            }
        }
        public async Task<bool> UpdateDriver(UpdateRequestDriverDto request, Guid driverId)
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

                if (driver == null)
                {
                    logger.LogWarning("Driver {DriverId} not found", driverId);
                    return false;
                }

                var bookings = await DriversBookings(driverId);

                foreach (var booking in bookings)
                {
                    booking.DriverIdFK = null;
                    booking.IsAvailable = true;
                }

                driver.IsDeleted = true;
                driver.DeletedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Driver {DriverId} deleted successfully with {Count} bookings released",
                    driverId, bookings.Count);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Failed to delete driver {DriverId}", driverId);
                return false;
            }
        }
        public async Task<List<Bookings>> DriversBookings(Guid driverId)
        {
            try
            {
                var bookings = await context.Bookings
                    .Where(b => b.DriverIdFK == driverId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.Completed)
                    .ToListAsync();

                return bookings;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve drivers' bookings");
                return [];
            }
        }
    }
}
