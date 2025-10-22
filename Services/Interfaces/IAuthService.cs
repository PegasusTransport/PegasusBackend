using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> LoginAsync(LoginRequestDto request);
        Task<ServiceResponse<TokenResponseDto?>> VerifyOTP2FAsync(Verify2FaDto verify2FaDto, HttpContext httpContext);
        Task<ServiceResponse<TokenResponseDto?>> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext httpContext);
        Task<ServiceResponse<bool>> LogoutAsync(HttpContext httpContext);
    }
}