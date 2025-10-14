using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces
{
    public interface IDriverService
    {
        Task<ServiceResponse<List<AllDriversDTO>>> GetAllDriversAsync();
        Task<ServiceResponse<DriverDTO>> GetDriverByIdAsync(Guid driverId);
        Task<ServiceResponse<DriverDTO>> GetDriverByUserIdAsync(HttpContext httpcontext);
        Task<ServiceResponse<CreatedDriverDTO>> CreateDriverAsync(CreateDriverDTO driver ,HttpContext httpContext);
        Task<ServiceResponse<UpdateDriverResponseDTO>> UpdateDriverAsync(Guid driverId, UpdateDriverDTO updatedDriver, HttpContext httpContext);
        Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId);
    }
}
