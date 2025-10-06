using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

public interface IAuthService
{
    Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginRequestDTO request, HttpContext httpContext);
    Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request);
    Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext httpContext);
    Task<ServiceResponse<bool>> LogoutAsync(HttpContext httpContext);
}