using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager) : IUserService
    {
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
                return ServiceResponse<RegistrationResponseDTO>.FailResponse("Failed to create user");
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
