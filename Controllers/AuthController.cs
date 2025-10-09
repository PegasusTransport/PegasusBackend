using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.Data;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponse?>> Login(LoginRequestDTO request) =>
            Generate.ActionResult<TokenResponse?>(await authService.LoginAsync(request, HttpContext));

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response.Message),
                HttpStatusCode.Unauthorized => Unauthorized(response.Message),
                HttpStatusCode.BadRequest => BadRequest(response.Message),
                _ => StatusCode((int)response.StatusCode, response.Message)
            };
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult> RefreshToken()
        {
            var response = await _authService.RefreshTokensFromCookiesAsync(HttpContext);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response.Data),
                HttpStatusCode.Unauthorized => Unauthorized(response.Message),
                HttpStatusCode.BadRequest => BadRequest(response.Message),
                _ => StatusCode((int)response.StatusCode, response.Message)
            };
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            var response = await _authService.LogoutAsync(HttpContext);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response.Message),
                HttpStatusCode.Unauthorized => Unauthorized(response.Message),
                _ => StatusCode((int)response.StatusCode, response.Message)
            };
        }
    }
}