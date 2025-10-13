using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IDriverRepo
    {
        Task<bool> CreateDriver(CreateDriverDTO request, string userId);
        Task<List<AllDriversDTO>> GetAllDrivers();
        Task<bool> UpdateDriver(UpdateDriverDTO request, string userId);
        Task<DriverDTO?> GetDriverByIdAsync(Guid id);

    }
}
