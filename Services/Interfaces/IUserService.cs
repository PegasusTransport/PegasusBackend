using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetEmployeeByValidRefreshTokenAsync(string refreshToken);
        public Task<ServiceResponse<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request);
    }
}
