using PegasusBackend.DTOs.DriverDTO;

namespace PegasusBackend.Repositorys.Implementations
{
    public interface IDriverRepo
    {
        Task<bool> CreateDriver(CreateDriverDTO request, string userId);
    }
}
