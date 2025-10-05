using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse?> LoginAsync(LoginReguest request);
        Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequest request);
    }
}
