using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IDriverRepo
    {
        Task<List<Drivers>> GetAllDriversAsync();
        Task<DriverResponseDto?> GetDriverByIdAsync(Guid id);
        Task<Drivers?> GetDriverEntityByIdAsync(Guid driverId);
        Task<DriverResponseDto?> GetDriverByUserIdAsync(string userId);
        Task<Guid?> CreateDriver(CreateRequestDriverDto request, string userId);
        Task<bool> UpdateDriver(UpdateRequestDriverDto request, Guid driverId);
        Task<bool> DeleteDriver(Guid driverId);


    }
}
