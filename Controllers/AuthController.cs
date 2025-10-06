using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.Data;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers.JwtCookieOptions;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginRequestDTO request)
        {
            var response = await authService.LoginAsync(request);

            // Change to mapper later
            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response.Data),
                HttpStatusCode.Unauthorized => Unauthorized(response.Message),
                HttpStatusCode.BadRequest => BadRequest(response.Message),
                _ => StatusCode((int)response.StatusCode, new { message = response.Message }) // Everything else
            };
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult> RefreshToken()
        {
            var response = await authService.RefreshTokensFromCookiesAsync();

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response.Data),
                HttpStatusCode.Unauthorized => Unauthorized(new { message = response.Message }),
                HttpStatusCode.BadRequest => BadRequest(new { message = response.Message }),
                _ => StatusCode((int)response.StatusCode, new { message = response.Message }) 
            };
        }
        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var response = await authService.LogoutAsync();

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(new { message = response.Message }),
                HttpStatusCode.Unauthorized => Unauthorized(new { message = response.Message }),
                _ => StatusCode((int)response.StatusCode, new { message = response.Message })
            };
        }
    }
}
