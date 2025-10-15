using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces
{
    public interface IDriverService
    {
        Task<ServiceResponse<List<AllDriversDto>>> GetAllDriversAsync();
        Task<ServiceResponse<DriverResponseDto>> GetDriverByIdAsync(Guid driverId);
        Task<ServiceResponse<DriverResponseDto>> GetDriverByUserIdAsync(HttpContext httpcontext);
        Task<ServiceResponse<CreatedResponseDriverDto>> CreateDriverAsync(CreateRequestDriverDto driver ,HttpContext httpContext);
        Task<ServiceResponse<UpdateDriverResponseDto>> UpdateDriverAsync(Guid driverId, UpdateDriverDto updatedDriver, HttpContext httpContext);
        Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId);
    }
}
