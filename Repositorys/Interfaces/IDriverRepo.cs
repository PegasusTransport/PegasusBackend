using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IDriverRepo
    {
        Task<List<AllDriversDTO>> GetAllDrivers();
        Task<DriverDTO?> GetDriverByIdAsync(Guid id);
        Task<bool> CreateDriver(CreateDriverDTO request, string userId);
        Task<bool> UpdateDriver(UpdateDriverDTO request, Guid driverId);
        Task<bool> DeleteDriver(Guid driverId);


    }
}
