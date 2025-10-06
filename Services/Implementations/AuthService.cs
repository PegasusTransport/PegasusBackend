using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers.JwtCookieOptions;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PegasusBackend.Services.Implementations
{
    // REFACTOR LATER
    public class AuthService(UserManager<User> _userManager, 
        IConfiguration _configuration, 
        IUserRepo repo, 
        IUserService userService, 
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor) : IAuthService

    {
        private HttpContext HttpContext => httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is not available"); // Ensure HttpContext is not null
        public async Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return ServiceResponse<TokenResponse?>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Invalid credentials"
                    );
                }

                // Check if user is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return ServiceResponse<TokenResponse?>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Account is locked"
                    );
                }

                var tokens = new TokenResponse
                {
                    AccessToken = await GenerateAccessToken(user),
                    RefreshToken = await CreateAndStoreRefreshToken(user)
                };
                HandleAuthenticationCookies.SetAuthenticationCookie(HttpContext, tokens.AccessToken, tokens.RefreshToken);
                return ServiceResponse<TokenResponse?>.SuccessResponse(HttpStatusCode.OK, tokens, "Login successful");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<TokenResponse?>.FailResponse(HttpStatusCode.InternalServerError, "Something went wrong");
            }
            
        }
        public async Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request)
        {
            try
            {
                var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
                if (user is null)
                {
                    return ServiceResponse<TokenResponse?>.FailResponse(HttpStatusCode.BadRequest, "Invalid refresh token");
                }

                var tokens = await CreateTokenResponse(user);
                return ServiceResponse<TokenResponse?>.SuccessResponse(HttpStatusCode.OK, tokens, "Tokens refreshed");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<TokenResponse?>.FailResponse(HttpStatusCode.InternalServerError, "Something went wrong");
            }
        }
        private async Task<User?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var user = await _userManager.FindByIdAsync(userId);  

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpireTime <= DateTime.UtcNow)
                return null;

            return user;

        }
        private async Task<TokenResponse> CreateTokenResponse(User user)
        {
            return new TokenResponse { AccessToken = await GenerateAccessToken(user), RefreshToken = await CreateAndStoreRefreshToken(user) };
        }
        private async Task<string> CreateAndStoreRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            await repo.HandleRefreshToken(user, refreshToken);
            return refreshToken;
        }
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);

        }
        private async Task<string> GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // For revoking Accesstoken later
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JwtSetting:Key")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var expire = _configuration.GetValue<int>("JwtSetting:Expire");

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JwtSetting:Issuer"),
                audience: _configuration.GetValue<string>("JwtSetting:Audience"),
                expires: DateTime.UtcNow.AddMinutes(expire),
                claims: claims,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public async Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync()
        {
            try
            {
                if (!HttpContext.Request.Cookies.TryGetValue(CookieNames.RefreshToken, out var refreshToken))
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.NotFound,
                        "No refresh token found"
                    );

                var user = await userService.GetUserByValidRefreshTokenAsync(refreshToken);
                if (user == null)
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid or expired refresh token"
                    );

                var refreshRequest = new RefreshTokenRequest { UserId = user.Id, RefreshToken = refreshToken };
                var tokenResponse = await RefreshTokensAsync(refreshRequest);

                if (tokenResponse == null || tokenResponse.Data == null)
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Token refresh failed"
                    );

                HandleAuthenticationCookies.SetAuthenticationCookie(HttpContext, tokenResponse.Data.AccessToken, tokenResponse.Data.RefreshToken);

                return ServiceResponse<string>.SuccessResponse(
                    HttpStatusCode.OK,
                    "Token refreshed successfully"
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<string>.FailResponse(
                    HttpStatusCode.InternalServerError, 
                    "Something went wrong"
                );
            }
        }
        public async Task<ServiceResponse<bool>> LogoutAsync()
        {

            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "User not found"
                    );
                }

                await userService.InvalidateRefreshTokenAsync(user);
                HandleAuthenticationCookies.ClearAuthenticationCookies(HttpContext);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Logout successful"
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong"
                );
            }

        }
       
    }
}
