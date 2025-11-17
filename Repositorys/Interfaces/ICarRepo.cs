using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface ICarRepo
    {
        public Task<bool> SaveCar(Cars car);
        Task<bool> UpdateCar(Cars car);
        public Task<Cars?> FindCarByRegNumberAsync(string regNumber);
    }
}
