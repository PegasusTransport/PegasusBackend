using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ServiceResponse<AuthResponseDto?>> VerifyTwoFaOTP(VerifyTwoFaDto verifyTwoFaDto, HttpContext httpContext);
        Task<ServiceResponse<TokenResponseDto?>> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext httpContext);
        Task<ServiceResponse<bool>> LogoutAsync(HttpContext httpContext);
    }
}