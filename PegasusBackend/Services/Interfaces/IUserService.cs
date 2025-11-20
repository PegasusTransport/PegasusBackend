using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<UserResponseDto>> GetUserByEmail(string email);
        Task<ServiceResponse<UserResponseDto>> GetLoggedInUser(HttpContext httpContext);
        Task<User?> GetUserByValidRefreshTokenAsync(string refreshToken);
        Task<ServiceResponse<List<AllUserResponseDto>>> GetAllUsers();
        Task<ServiceResponse<bool>> RegisterUserAsync(RegistrationRequestDto request);
        Task<ServiceResponse<bool>> ResendVerificationEmail(string email);
        Task<ServiceResponse<string>> ConfirmUserEmailAsync(string token, string email);
        Task<ServiceResponse<bool>> DeleteUserAsync(HttpContext httpContext);
        Task<ServiceResponse<bool>> InvalidateRefreshTokenAsync(User user);
        Task<ServiceResponse<UpdateUserResponseDto>> UpdateUserAsync(UpdateUserRequestDto request, HttpContext httpContext); 
    }
}
