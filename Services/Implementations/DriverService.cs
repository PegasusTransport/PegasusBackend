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
        public Task<ServiceResponse<Drivers>> GetDriverByIdAsync(int driverId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> DeleteDriverAsync(int driverId)
        {
            throw new NotImplementedException();
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
        public Task<ServiceResponse<Drivers>> UpdateDriverAsync(int driverId, Drivers updatedDriver)
        {
            throw new NotImplementedException();
        }


    }
}
