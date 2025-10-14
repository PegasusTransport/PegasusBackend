using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Bcpg;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers.JwtCookieOptions;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace PegasusBackend.Services.Implementations
{
    public class UserService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserRepo userRepo, ILogger<UserService> logger) : IUserService
    {
        public async Task<ServiceResponse<List<AllUserDTO>>> GetAllUsers()
        {
            try
            {
                var allUsers = await userManager.Users.
                    Select(u => new AllUserDTO
                    {
                        UserName = u.UserName!,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email!

                    }).ToListAsync();


                string message = allUsers.Count > 0 
                    ? $"Found {allUsers.Count} User(s)"
                    : "No Users found";

                return ServiceResponse<List<AllUserDTO>>.SuccessResponse(
                   HttpStatusCode.OK,
                   allUsers,
                   message
               );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing tokens from cookies");
                return ServiceResponse<List<AllUserDTO>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong"
                );
            }

        }
        public async Task<ServiceResponse<RegistrationResponseDTO>> RegisterUserAsync(RegistrationRequestDTO request)
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
                    UserName = request.UserName.Trim(),
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.Trim(),  
                    PhoneNumber = request.PhoneNumber.Trim(),
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
        public async Task<ServiceResponse<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserRequestDTO request, HttpContext httpContext)
        {
            try
            {
                var user = await GetUserFromCookieAsync(httpContext);

                if (user == null)
                {
                    return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                            HttpStatusCode.NotFound,
                            "User not found"
                        );
                }

                if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
                {
                    var existingUser = await userManager.FindByNameAsync(request.UserName);
                    if (existingUser != null)
                    {
                        return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                            HttpStatusCode.BadRequest,
                            "Username already exists"
                        );
                    }
                    user.UserName = request.UserName;
                }

                if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
                {
                    var existingUser = await userManager.FindByEmailAsync(request.Email);
                    if (existingUser != null)
                    {
                        return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                            HttpStatusCode.BadRequest,
                            "Email already exists"
                        );
                    }
                    user.Email = request.Email;
                }
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
                {
                    var existingUser = await userManager.Users
                        .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
                    if (existingUser != null)
                    {
                        return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                            HttpStatusCode.BadRequest,
                            "PhoneNumber already exists"
                        );
                    }
                    user.PhoneNumber = request.PhoneNumber;
                }
                if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;


                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                        HttpStatusCode.BadRequest,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                }

                var response = new UpdateUserResponseDTO()
                {
                    UserName = user.UserName!
                };

                return ServiceResponse<UpdateUserResponseDTO>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                   $"Updated user {user.UserName}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Server error");
                return ServiceResponse<UpdateUserResponseDTO>.FailResponse(
                            HttpStatusCode.InternalServerError,
                            "Server error"
                        );
            }
        }
        public async Task<ServiceResponse<bool>> DeleteUserAsync(HttpContext httpContext)
        {
            try
            {
                var user = await GetUserFromCookieAsync(httpContext);


                if (user == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                         HttpStatusCode.Unauthorized,
                         "Not authorizad"
                     );
                }

                if (await userRepo.UserHasBookings(user))
                {
                    return ServiceResponse<bool>.FailResponse(
                         HttpStatusCode.BadRequest,
                         "Cant delete user with bookings, cancel bookings before deleting account"
                     );
                }
                user.Email = $"{Guid.NewGuid()}@DELETED.Com";
                user.NormalizedEmail = user.Email.ToUpper();
                user.UserName = $"deleted_{Guid.NewGuid():N}";
                user.NormalizedUserName = user.UserName.ToUpperInvariant();
                user.PhoneNumber = null;
                user.FirstName = "Deleted";
                user.LastName = "User";
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                await InvalidateRefreshTokenAsync(user);
                await userManager.UpdateAsync(user);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "User Deleted"
                    );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete");
                return ServiceResponse<bool>.FailResponse(
                   HttpStatusCode.InternalServerError,
                   "User failed to delete"
                   );
            }
        }
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
        public async Task<User?> GetUserByValidRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            var user = await userRepo.GetUserByRefreshToken(refreshToken);

            if (user?.RefreshTokenExpireTime <= DateTime.UtcNow)
                return null;

            return user;
        }
        private async Task<User?> GetUserFromCookieAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.Cookies.TryGetValue(CookieNames.RefreshToken, out var refreshToken))
            {
                return null;
            }

            var user = await GetUserByValidRefreshTokenAsync(refreshToken);

            return user;
        }
        public async Task<ServiceResponse<UserDTO>> GetUserById(string userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<UserDTO>.FailResponse(
                    HttpStatusCode.NotFound,
                    "User not found"
                    );
                }

                var userResponse = new UserDTO()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName!,
                    UserName = user.UserName!,
                    Email = user.Email!
                };
                return ServiceResponse<UserDTO>.SuccessResponse(
                    HttpStatusCode.OK,
                    userResponse,
                    "User not found"
                    );
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<UserDTO>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong");
            }
        }
    }
}
