using PegasusBackend.DTOs.DriverDTO;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IDriverRepo
    {
        Task<bool> CreateDriver(CreateDriverDTO request, string userId);
        Task<List<AllDriversDTO>> GetAllDrivers();
    }
}
