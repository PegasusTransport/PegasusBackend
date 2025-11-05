using Mailjet.Client.Resources.SMS;
using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class UserRepo(AppDBContext context, ILogger<UserRepo> _logger, IConfiguration configuration) : IUserRepo
    {
        public async Task<User?> GetUserByRefreshToken(string refreshtoken)
        {
            return await context.Users.FirstOrDefaultAsync(e => e.RefreshToken == refreshtoken);
        }
        public async Task<bool> HandleRefreshToken(User user, string? refreshtoken)
        {
            var expireRefreshToken = configuration.GetValue<int>("JwtSetting:RefreshTokenExpire");
            user.RefreshToken = refreshtoken;
            user.RefreshTokenExpireTime = DateTime.UtcNow.AddDays(expireRefreshToken);
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UserHasBookings(User user)
        {
            var bookings = await context.Bookings
                .Where(b => b.UserIdFk == user.Id && b.Status != BookingStatus.Cancelled).ToListAsync();

            _logger.LogInformation($"Found {bookings.Count} bookings");

            return bookings.Count != 0;
        }
    }
}
