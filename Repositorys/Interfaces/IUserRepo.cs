using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IUserRepo
    {
        Task<bool> HandleRefreshToken(User user, string? refreshtoken);
        Task<User?> GetUserByRefreshToken(string refreshtoken);
    }
}
