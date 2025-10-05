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
    public class AuthController(IAuthService authService, IUserService userService, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginReguest request)
        {
            var result = await authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result.Message);
               
            }
            if (result.Data is null)
            {
                return Unauthorized("Token saknas i svaret.");
            }

            HandleAuthenticationCookies.SetAuthenticationCookie(HttpContext, result.Data.AccessToken, result.Data.RefreshToken);
            return Ok(new { message = "Logged in successfully" }); // Change to token if needed, but they will be stored in cookies

        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<ApiResponse>> RefreshToken()
        {
            var result = await authService.RefreshTokensFromCookiesAsync(HttpContext);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(new { message = result.Message });
        }

    }
}
