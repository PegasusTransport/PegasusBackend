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

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<string>> RefreshToken() =>
            Generate.ActionResult(await authService.RefreshTokensFromCookiesAsync(HttpContext));

        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult<bool>> Logout() =>
            Generate.ActionResult(await authService.LogoutAsync(HttpContext));
    }
}