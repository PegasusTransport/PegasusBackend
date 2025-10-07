using Microsoft.AspNetCore.Identity;
using PegasusBackend.Data;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Implementations;

namespace PegasusBackend.Repositorys.Interfaces
{
    public class DriverRepo(AppDBContext context, ILogger<DriverRepo> logger, UserManager<User> userManager) : IDriverRepo
    {
        public async Task<bool> CreateDriver(CreateDriverDTO request, string userId)
        {
         
			try
			{
                var newDriver = new Drivers
                {
                    DriverId = Guid.NewGuid(),
                    UserId = userId,
                    ProfilePicture = request.ProfilePicture
                };
                context.Drivers.Add(newDriver);
                await context.SaveChangesAsync();
                return true;
            }
			catch (Exception ex)
			{
                logger.LogError(ex.Message);
				return false;
			}
        }
    }
}
