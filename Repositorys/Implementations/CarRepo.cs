using PegasusBackend.Data;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class CarRepo(AppDBContext context) : ICarRepo
    {
        public Task SaveCar()
        {
            throw new NotImplementedException();
        }
    }
}
