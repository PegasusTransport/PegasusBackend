using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IDriverService
    {
        Task<ServiceResponse<List<Drivers>>> GetAllDriversAsync();
        Task<ServiceResponse<CreatedDriverDTO>> CreateDriverAsync(CreateDriverDTO driver ,HttpContext httpContext);
        Task<ServiceResponse<Drivers>> UpdateDriverAsync(int driverId, Drivers updatedDriver);
        Task<ServiceResponse<bool>> DeleteDriverAsync(int driverId);

    }
}
