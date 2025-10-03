using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IUserService
    {
        public Task<ServiceResponse<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request);
    }
}
