using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IUserRepo
    {
        Task<bool> SaveRefreshToken(User user, string? refreshtoken);
        Task<User?> GetUserByRefreshToken(string refreshtoken);
    }
}
