using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByValidRefreshTokenAsync(string refreshToken);
        Task<ServiceResponse<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request);
        Task<ServiceResponse<bool>> InvalidateRefreshTokenAsync(User user);
    }
}
