using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginReguest request);
        Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request);
        Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext context);
    }
}
