using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class UserRepo(AppDBContext context) : IUserRepo
    {
        public async Task<User?> GetUserByRefreshToken(string refreshtoken)
        {
            return await context.Users.FirstOrDefaultAsync(e => e.RefreshToken == refreshtoken);
        }

        public async Task<bool> SaveRefreshToken(User user, string? refreshtoken)
        {
            user.RefreshToken = refreshtoken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
