using Azure;
using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserRepo userRepo, ILogger<UserService> logger, IMailjetEmailService mailjetEmailService) : IUserService
    {
        public async Task<User?> GetUserByValidRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var user = await userRepo.GetUserByRefreshToken(refreshToken);

            if (user?.RefreshTokenExpireTime <= DateTime.UtcNow)
                return null;

            return user;
        }

        public async Task<ServiceResponse<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request)
        {
            try
            {
                var existingUser = await userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return ServiceResponse<RegistrationResponseDTO>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "User exist"
                    );
                }

                var newUser = new User
                {
                    UserName = request.UserName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    //TwoFactorEnabled = true, // WHEN  we WANT 2FA 

                    SecurityStamp = Guid.NewGuid().ToString(),
                };

                // Use role from request
                if (!await roleManager.RoleExistsAsync(request.Role.ToString()))
                {
                    return ServiceResponse<RegistrationResponseDTO>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Role doesn't exist"
                    );
                }

                var result = await userManager.CreateAsync(newUser, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return ServiceResponse<RegistrationResponseDTO>.FailResponse(
                        HttpStatusCode.BadRequest, 
                        $"Failed to create user: {errors}"
                    );
                }

            

                await userManager.AddToRoleAsync(newUser, request.Role.ToString());

                // If the user is a customer, we send them a customer welcome template, if the are a driver, they get another template!
                // Send a email about confirming their account. 
                await mailjetEmailService.SendEmailAsync(
                    newUser.Email,
                    MailjetTemplateType.Welcome,
                    new WelcomeDto { firstname = newUser.FirstName },
                    MailjetSubjects.Welcome
                );

                var response = new RegistrationResponseDTO
                {
                    FirstName = newUser.FirstName,
                    Email = newUser.Email,
                };

                return ServiceResponse<RegistrationResponseDTO>.SuccessResponse(
                    HttpStatusCode.OK,response, 
                    "Created User");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<RegistrationResponseDTO>.FailResponse(
                    HttpStatusCode.InternalServerError, 
                    "Something went wrong");
            }
        }
        // Refactor
        public async Task<ServiceResponse<bool>> InvalidateRefreshTokenAsync(User user)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpireTime = null;

            return ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK, 
                await userRepo.HandleRefreshToken(user, null), 
                "Ok"
            );

        }
    }
}
