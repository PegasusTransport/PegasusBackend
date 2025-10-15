using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IDriverRepo
    {
        Task<List<AllDriversDto>> GetAllDrivers();
        Task<DriverResponseDto?> GetDriverByIdAsync(Guid id);
        Task<DriverResponseDto?> GetDriverByUserIdAsync(string userId);
        Task<bool> CreateDriver(CreateRequestDriverDto request, string userId);
        Task<bool> UpdateDriver(UpdateDriverDto request, Guid driverId);
        Task<bool> DeleteDriver(Guid driverId);


    }
}
