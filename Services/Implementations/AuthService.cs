using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PegasusBackend.Services.Implementations
{
    public class AuthService(UserManager<User> _userManager, IConfiguration _configuration): IAuthService
    {
        public async Task<ServiceResponse<TokenResponse?>> LoginAsync(LoginReguest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return ServiceResponse<TokenResponse?>.FailResponse("Wrong password or email");
            }
            if (user.Email != request.Email)
            {
                return ServiceResponse<TokenResponse?>.FailResponse("Wrong password or email");
            }
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return ServiceResponse<TokenResponse?>.FailResponse("wrong password or email");
            }
            var tokens = new TokenResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = " "
            };

            return ServiceResponse<TokenResponse?>.SuccessResponse(tokens, "Created Tokens");
        }

        public Task<ServiceResponse<TokenResponse?>> RefreshTokensAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // For revoking Accesstoken later
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JwtSetting:Key")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JwtSetting:Issuer"),
                audience: _configuration.GetValue<string>("JwtSetting:Audience"),
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:Expire")),
                claims: claims,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
