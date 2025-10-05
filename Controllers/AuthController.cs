using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.Data;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers.JwtCookieOptions;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, UserManager<User> _userManager, IUserService userService) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginReguest request)
        {
            var result = await authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(ApiResponse.Error(result.Message));
               
            }
            if (result.Data is null)
            {
                return Unauthorized(ApiResponse.Error(result.Message));
            }

            HandleAuthenticationCookies.SetAuthenticationCookie(HttpContext, result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(new { message = "Logged in successfully" }); // Change to token if needed, but they will be stored in cookies

        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<ApiResponse>> RefreshToken()
        {
            var result = await authService.RefreshTokensFromCookiesAsync(HttpContext);

            if (!result.Success)
                return Unauthorized(ApiResponse.Error(result.Message));

            return Ok(ApiResponse.Ok("Token refreshed successfully"));
        }
        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Unauthorized(ApiResponse.Error("User not authenticated"));
                }

                await userService.InvalidateRefreshTokenAsync(user);
                HandleAuthenticationCookies.ClearAuthenticationCookies(HttpContext);
                return Ok(ApiResponse.Ok("Logout successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Error($"Logout failed, {ex.Message}"));
            }
        }

    }
}
