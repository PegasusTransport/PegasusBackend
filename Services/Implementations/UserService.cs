using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserRepo userRepo) : IUserService
    {
        public async Task<User?> GetEmployeeByValidRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var employee = await userRepo.GetUserByRefreshToken(refreshToken);

            if (employee?.RefreshTokenExpireTime <= DateTime.UtcNow)
                return null;

            return employee;
        }

        public async Task<ServiceResponse<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ServiceResponse<RegistrationResponseDTO>.FailResponse("User exist");
            }

            var newUser = new User
            {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                //TwoFactorEnabled = true, // IF WE WANT 2FA 
                
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            // Use role from request
            if (!await roleManager.RoleExistsAsync(request.Role.ToString()))
            {
                return ServiceResponse<RegistrationResponseDTO>.FailResponse("Role doesn't exist");
            }

            var result = await userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResponse<RegistrationResponseDTO>.FailResponse($"Failed to create user: {errors}");
            }

            

            await userManager.AddToRoleAsync(newUser, request.Role.ToString());

            var response = new RegistrationResponseDTO
            {
                FirstName = newUser.FirstName,
                Email = newUser.Email,
            };

            return ServiceResponse<RegistrationResponseDTO>.SuccessResponse(response, "Created User");
        }
        
    }
}
