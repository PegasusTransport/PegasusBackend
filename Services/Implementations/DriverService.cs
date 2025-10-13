using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class DriverService(UserManager<User> _userManager, IDriverRepo driverRepo,ILogger<DriverService> logger) : IDriverService
    {
        public async Task<ServiceResponse<CreatedDriverDTO>> CreateDriverAsync(CreateDriverDTO request, HttpContext httpContext)
        {
            try
            {
                var user = await _userManager.GetUserAsync(httpContext.User);

                if (user == null)
                    return ServiceResponse<CreatedDriverDTO>.FailResponse(
                        HttpStatusCode.NotFound,
                        "No user with that mail"
                    );

                if (!await _userManager.IsInRoleAsync(user, UserRoles.Driver.ToString()))
                    return ServiceResponse<CreatedDriverDTO>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Not role as Driver"
                    );

                if (await driverRepo.CreateDriver(request, user.Id))
                {
                    var newDriver = new CreatedDriverDTO
                    {
                        DriverName = user.FirstName,
                        Email = user.Email!,
                    };
                    return ServiceResponse<CreatedDriverDTO>.SuccessResponse(HttpStatusCode.OK, newDriver );
                }
                else
                {
                    return ServiceResponse<CreatedDriverDTO>.FailResponse(HttpStatusCode.BadRequest, "Failed to created driver");
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return ServiceResponse<CreatedDriverDTO>.FailResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
        public async Task<ServiceResponse<List<AllDriversDTO>>> GetAllDriversAsync()
        {
            try
            {
                var drivers = await driverRepo.GetAllDrivers();

                string message = drivers.Count > 0
                    ? $"Found {drivers.Count} driver(s)"
                    : "No drivers found";

                return ServiceResponse<List<AllDriversDTO>>.SuccessResponse(
                    HttpStatusCode.OK,
                    drivers,
                    message
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Service error in GetAllDriversAsync");
                return ServiceResponse<List<AllDriversDTO>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to retrieve drivers"
                );
            }

        }
        public async Task<ServiceResponse<DriverDTO>> GetDriverByIdAsync(Guid driverId)
        {
            try
            {
                var driver = await driverRepo.GetDriverByIdAsync(driverId);

                if (driver == null)
                {
                    return ServiceResponse<DriverDTO>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Driver not found"
                    );
                }

                return ServiceResponse<DriverDTO>.SuccessResponse(
                    HttpStatusCode.OK,
                    driver,
                    "Driver retrieved"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving driver {DriverId}", driverId);
                return ServiceResponse<DriverDTO>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to retrieve driver"
                );
            }
        }

        public async Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId)
        {
            try
            {
                var driver = await driverRepo.GetDriverByIdAsync(driverId);

                if (driver == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Driver not found"
                    );
                }

                var result = await driverRepo.DeleteDriver(driverId);

                if (!result)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to delete driver"
                    );
                }

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Driver deleted"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting driver {DriverId}", driverId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to delete driver"
                );
            }
        }

        public async Task<ServiceResponse<UpdateDriverResponseDTO>> UpdateDriverAsync(Guid driverId, UpdateDriverDTO updatedDriver)
        {
            try
            {
                var result = await driverRepo.UpdateDriver(updatedDriver, driverId);

                if (!result)
                {
                    return ServiceResponse<UpdateDriverResponseDTO>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to update driver"
                    );
                }

                var driver = await driverRepo.GetDriverByIdAsync(driverId);

                var response = new UpdateDriverResponseDTO
                {
                    DriverId = driverId,
                    ProfilePicture = driver?.ProfilePicture ?? string.Empty,
                    CarId = driver?.CarId
                };

                return ServiceResponse<UpdateDriverResponseDTO>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Driver updated"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating driver {DriverId}", driverId);
                return ServiceResponse<UpdateDriverResponseDTO>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to update driver"
                );
            }
        }


    }
}
