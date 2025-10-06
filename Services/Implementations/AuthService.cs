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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PegasusBackend.Services.Implementations
{
    // REFACTOR LATER
    public class AuthService(UserManager<User> _userManager, IConfiguration _configuration, IUserRepo repo, IUserService userService): IAuthService
    {
        public async Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginReguest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return ServiceResponse<TokenResponse?>.FailResponse("Invalid credentials");
            }

            // Kontrollera om användaren är låst
            if (await _userManager.IsLockedOutAsync(user))
            {
                return ServiceResponse<TokenResponse?>.FailResponse("Account is locked");
            }

            var tokens = new TokenResponse
            {
                AccessToken = await GenerateAccessToken(user),
                RefreshToken = await CreateAndStoreRefreshToken(user)
            };

            return ServiceResponse<TokenResponse?>.SuccessResponse(tokens, "Login successful");
        }
        public async Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return ServiceResponse<TokenResponse?>.FailResponse("Invalid refresh token");
            }

            var tokens = await CreateTokenResponse(user);
            return ServiceResponse<TokenResponse?>.SuccessResponse(tokens, "Tokens refreshed");
        }
        private async Task<User?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var user = await _userManager.FindByIdAsync(userId);  

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpireTime <= DateTime.UtcNow)
            {
                return null;
            }

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
        public async Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext context)
        {
            if (!context.Request.Cookies.TryGetValue(CookieNames.RefreshToken, out var refreshToken))
                return ServiceResponse<string>.FailResponse("No refresh token found");

            var user = await userService.GetUserByValidRefreshTokenAsync(refreshToken);
            if (user == null)
                return ServiceResponse<string>.FailResponse("Invalid or expired refresh token");

            var refreshRequest = new RefreshTokenRequest { UserId = user.Id, RefreshToken = refreshToken };
            var tokenResponse = await RefreshTokensAsync(refreshRequest);

            if (tokenResponse == null || tokenResponse.Data == null)
                return ServiceResponse<string>.FailResponse("Token refresh failed");

            HandleAuthenticationCookies.SetAuthenticationCookie(context, tokenResponse.Data.AccessToken, tokenResponse.Data.RefreshToken);

            return ServiceResponse<string>.SuccessResponse("Token refreshed successfully");
        }

    }
}
