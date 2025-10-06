using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginRequestDTO request);
        Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request);
        Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync();
        Task<ServiceResponse<bool>> LogoutAsync();
    }
}
