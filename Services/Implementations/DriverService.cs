using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.ComponentModel;
using System.Net;
using System.Security.Claims;

namespace PegasusBackend.Services.Implementations
{
    public class DriverService(UserManager<User> _userManager, IDriverRepo driverRepo,ILogger<DriverService> logger) : IDriverService
    {
        public async Task<ServiceResponse<CreatedResponseDriverDto>> CreateDriverAsync(CreateRequestDriverDto request, HttpContext httpContext)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProfilePicture))
                    return ServiceResponse<CreatedResponseDriverDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Profile picture is required"
                    );

                var user = await _userManager.GetUserAsync(httpContext.User);

                if (user == null)
                    return ServiceResponse<CreatedResponseDriverDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "No user with that mail"
                    );

                if (!await _userManager.IsInRoleAsync(user, UserRoles.Driver.ToString()))
                    return ServiceResponse<CreatedResponseDriverDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Not role as Driver"
                    );
                if (user.IsDeleted)
                    return ServiceResponse<CreatedResponseDriverDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Cannot create driver for deleted user"
                    );
                if (await driverRepo.CreateDriver(request, user.Id))
                {
                    var newDriver = new CreatedResponseDriverDto
                    {
                        DriverName = user.FirstName,
                        Email = user.Email!,
                    };
                    return ServiceResponse<CreatedResponseDriverDto>.SuccessResponse(HttpStatusCode.OK, newDriver );
                }
                else
                {
                    return ServiceResponse<CreatedResponseDriverDto>.FailResponse(HttpStatusCode.BadRequest, "Failed to created driver");
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return ServiceResponse<CreatedResponseDriverDto>.FailResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
        public async Task<ServiceResponse<List<AllDriversDto>>> GetAllDriversAsync()
        {
            try
            {
                var drivers = await driverRepo.GetAllDrivers();

                string message = drivers.Count > 0
                    ? $"Found {drivers.Count} driver(s)"
                    : "No drivers found";

                return ServiceResponse<List<AllDriversDto>>.SuccessResponse(
                    HttpStatusCode.OK,
                    drivers,
                    message
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Service error in GetAllDriversAsync");
                return ServiceResponse<List<AllDriversDto>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to retrieve drivers"
                );
            }

        }
        public async Task<ServiceResponse<DriverResponseDto>> GetDriverByUserIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return ServiceResponse<DriverResponseDto>.FailResponse(
                       HttpStatusCode.NotFound,
                       "Failed to find user"
                   );
                }
                var driver = await driverRepo.GetDriverByUserIdAsync(user.Id);

                if (driver == null)
                {
                    return ServiceResponse<DriverResponseDto>.FailResponse(
                       HttpStatusCode.NotFound,
                       "Failed to find driver"
                   );
                }
                var repsonse = new DriverResponseDto
                {
                    DriverId = driver.DriverId,
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    ProfilePicture = driver.ProfilePicture,
                    CarId = driver.CarId
                };
                return ServiceResponse<DriverResponseDto>.SuccessResponse(
                       HttpStatusCode.OK,
                       repsonse,
                       "Found driver"
                   );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error geting driver");
                return ServiceResponse<DriverResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Error while to get driver"
                );
            }


        }
        public async Task<ServiceResponse<DriverResponseDto>> GetDriverByIdAsync(Guid driverId)
        {
            try
            {
                var driver = await driverRepo.GetDriverByIdAsync(driverId);

                if (driver == null)
                {
                    return ServiceResponse<DriverResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Driver not found"
                    );
                }

                return ServiceResponse<DriverResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    driver,
                    "Driver retrieved"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving driver {DriverId}", driverId);
                return ServiceResponse<DriverResponseDto>.FailResponse(
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
        public async Task<ServiceResponse<UpdateDriverResponseDto>> UpdateDriverAsync(Guid driverId, UpdateRequestDriverDto updatedDriver, HttpContext httpContext)
        {
            try
            {
                if (!await CanUpdateDriver(httpContext, driverId))
                {
                    return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "You don't have permission to update this driver"
                    );
                }

                var result = await driverRepo.UpdateDriver(updatedDriver, driverId);
                if (!result)
                {
                    return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to update driver"
                    );
                }

                var driver = await driverRepo.GetDriverByIdAsync(driverId);
                var response = new UpdateDriverResponseDto
                {
                    DriverId = driverId,
                    ProfilePicture = driver?.ProfilePicture ?? string.Empty,
                    CarId = driver?.CarId
                };

                return ServiceResponse<UpdateDriverResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Driver updated"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating driver {DriverId}", driverId);
                return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to update driver"
                );
            }
        }
        private async Task<bool> CanUpdateDriver(HttpContext httpContext, Guid driverId)
        {
            var currentUser = await _userManager.GetUserAsync(httpContext.User);
            if (currentUser == null) return false;

            if (await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin.ToString()))
                return true;
            var currentDriver = await driverRepo.GetDriverByUserIdAsync(currentUser.Id);
            return currentDriver?.DriverId == driverId;
        }
    }
}
