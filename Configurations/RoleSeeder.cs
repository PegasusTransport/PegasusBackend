using Microsoft.AspNetCore.Identity;
using PegasusBackend.Configurations;
using PegasusBackend.Models.Roles;

namespace PegasusBackend.Configurations
{
    public static class RoleSeeder
    {
        public static async Task CreateRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Hämta alla roller från enum
            var roles = Enum.GetNames(typeof(UserRoles));

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{role}'");
                    }
                }
            }
        }
    }
}


