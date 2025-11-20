using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.JwtCookieOptions;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
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
    public class AuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        IUserRepo repo,
        IUserService userService,
        ILogger<AuthService> logger,
        IMailjetEmailService mailjetEmailService) : IAuthService
    {
        public async Task<ServiceResponse<LoginResponseDto>> SendTwoFaAsync(LoginRequestDto request)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(request.Email);

  
                bool isPasswordValid = false;
                if (user != null)
                {
                    isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
                }

                if (user == null || !isPasswordValid)
                {
                    return ServiceResponse<LoginResponseDto>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Invalid credentials"
                    );
                }

                if (user.IsDeleted || !user.EmailConfirmed)
                {
                    return ServiceResponse<LoginResponseDto>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Invalid credentials" 
                    );
                }

                if (await userManager.IsLockedOutAsync(user))
                {
                    return ServiceResponse<LoginResponseDto>.FailResponse(
                        HttpStatusCode.Forbidden, 
                        "Account is locked. Please contact support."
                    );
                }

                var otpToken = await userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);

                await mailjetEmailService.SendEmailAsync(
                    user.Email!,
                    MailjetTemplateType.TwoFA,
                    new TwoFARequestDto
                    {
                        Firstname = user.FirstName,
                        VerificationCode = otpToken
                    },
                    MailjetSubjects.TwoFA);


                return ServiceResponse<LoginResponseDto>.SuccessResponse(
                        HttpStatusCode.OK,
                        new LoginResponseDto
                        {
                            Email = user.Email!
                        },
                        $"A verification Code has been sent to {user.Email}");


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return ServiceResponse<LoginResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }

        public async Task<ServiceResponse<bool?>> VerifyTwoFaOTPAndLogin(VerifyTwoFaDto verifyTwoFaDto, HttpContext httpContext)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(verifyTwoFaDto.Email);
                var isValidOtp = await userManager.VerifyTwoFactorTokenAsync(user!, TokenOptions.DefaultEmailProvider, verifyTwoFaDto.VerificationCode);

                if (!isValidOtp)
                {
                    return ServiceResponse<bool?>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Wrong verifaction code"
                );
                }

                var tokens = new TokenResponseDto
                {
                    AccessToken = await GenerateAccessToken(user!),
                    RefreshToken = await CreateAndStoreRefreshToken(user!),
                };

                HandleAuthenticationCookies.SetAuthenticationCookie(
                    httpContext,
                    tokens.AccessToken,
                    tokens.RefreshToken
                    );

                return ServiceResponse<bool?>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Login successful"
                );
            }
            catch (Exception ex)
            {

                logger.LogError(ex, "Error during login for email: {Email}", verifyTwoFaDto.Email); 
                return ServiceResponse<bool?>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }
        // REMOVE IN PRODUCTION
        public async Task<ServiceResponse<bool?>> DevLoginAsync(LoginRequestDto request, HttpContext httpContext)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(request.Email);
                if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
                {
                    return ServiceResponse<bool?>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Invalid credentials"
                    );
                }
                var tokens = new TokenResponseDto
                {
                    AccessToken = await GenerateAccessToken(user),
                    RefreshToken = await CreateAndStoreRefreshToken(user),
                };
                HandleAuthenticationCookies.SetAuthenticationCookie(
                    httpContext,
                    tokens.AccessToken,
                    tokens.RefreshToken
                    );

                return ServiceResponse<bool?>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Login successful"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during dev login for email: {Email}", request.Email);
                return ServiceResponse<bool?>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }

        public async Task<ServiceResponse<TokenResponseDto?>> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            try
            {
                var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
                if (user is null)
                {
                    return ServiceResponse<TokenResponseDto?>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Invalid refresh token"
                    );
                }

                var tokens = await CreateTokenResponse(user);
                return ServiceResponse<TokenResponseDto?>.SuccessResponse(
                    HttpStatusCode.OK,
                    tokens,
                    "Tokens refreshed"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing tokens for user: {UserId}", request.UserId);
                return ServiceResponse<TokenResponseDto?>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong"
                );
            }
        }

        public async Task<ServiceResponse<string>> RefreshTokensFromCookiesAsync(HttpContext httpContext)
        {
            try
            {
                if (!httpContext.Request.Cookies.TryGetValue(CookieNames.RefreshToken, out var refreshToken))
                {
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "No refresh token found"
                    );
                }

                var user = await userService.GetUserByValidRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Invalid or expired refresh token"
                    );
                }

                var refreshRequest = new RefreshTokenRequestDto
                {
                    UserId = user.Id,
                    RefreshToken = refreshToken
                };

                var tokenResponse = await RefreshTokensAsync(refreshRequest);

                if (tokenResponse?.Data == null)
                {
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "Token refresh failed"
                    );
                }

                HandleAuthenticationCookies.SetAuthenticationCookie(
                    httpContext,
                    tokenResponse.Data.AccessToken,
                    tokenResponse.Data.RefreshToken
                );

                return ServiceResponse<string>.SuccessResponse(
                    HttpStatusCode.OK,
                    "OK",
                    "Token refreshed successfully"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing tokens from cookies");
                return ServiceResponse<string>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong"
                );
            }
        }

        public async Task<ServiceResponse<bool>> LogoutAsync(HttpContext httpContext)
        {
            try
            {
                var user = await userManager.GetUserAsync(httpContext.User);

                if (user == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "User not found"
                    );
                }

                await userService.InvalidateRefreshTokenAsync(user);
                HandleAuthenticationCookies.ClearAuthenticationCookies(httpContext);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Logout successful"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout");
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong"
                );
            }
        }

        // Private helper methods
        private async Task<User?> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user is null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpireTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = await GenerateAccessToken(user),
                RefreshToken = await CreateAndStoreRefreshToken(user)
            };
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
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSetting:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var expire = configuration.GetValue<int>("JwtSetting:AccessTokenExpire");

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["JwtSetting:Issuer"],
                audience: configuration["JwtSetting:Audience"],
                expires: DateTime.UtcNow.AddMinutes(expire),
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public ServiceResponse<SessionLifeTimeDto> GetSessionLifetime()
        {
            try
            {
                var sessionLifeTime = new SessionLifeTimeDto
                {
                    RefreshTokenLifetime = configuration.GetValue<int>("JwtSetting:RefreshTokenExpire")
                };
                return ServiceResponse<SessionLifeTimeDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    sessionLifeTime,
                    $"Session lifetime, {sessionLifeTime.RefreshTokenLifetime} Represents days"
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving session lifetime");
                return ServiceResponse<SessionLifeTimeDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred"
                );
            }
        }
    }
}
