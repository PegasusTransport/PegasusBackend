using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDTO>> GetUserByEmail(string email);
        Task<User?> GetUserByValidRefreshTokenAsync(string refreshToken);
        Task<ServiceResponse<List<AllUserDTO>>> GetAllUsers();
        Task<ServiceResponse<RegistrationResponseDTO>> RegisterUserAsync(RegistrationRequestDTO request);
        Task<ServiceResponse<bool>> DeleteUserAsync(HttpContext httpContext);
        Task<ServiceResponse<bool>> InvalidateRefreshTokenAsync(User user);
        Task<ServiceResponse<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserRequestDTO request, HttpContext httpContext); 
    }
}
