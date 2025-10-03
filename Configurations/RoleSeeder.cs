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

// Create Roles for identity - call this in Program.cs after building the app to get roles created
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        await RoleSeeder.CreateRolesAsync(services);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "Ett fel uppstod när roller skulle skapas");
//    }
//}

