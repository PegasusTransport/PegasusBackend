using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.Data;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Threading.Tasks.Dataflow;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IPasswordResetService passwordResetService) : ControllerBase
    {
        [HttpPost("Login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request) =>
            Generate.ActionResult<LoginResponseDto>(await authService.LoginAsync(request));

        [HttpPost("VerifyTwoFA")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<AuthResponseDto?>> VerifyTwoFA(VerifyTwoFaDto request) =>
            Generate.ActionResult<AuthResponseDto?>(await authService.VerifyTwoFaOTP(request, HttpContext));

        [HttpPost("RefreshToken")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<string>> RefreshToken() =>
            Generate.ActionResult(await authService.RefreshTokensFromCookiesAsync(HttpContext));

        [HttpPost("Logout")]
        [Authorize]
        public async Task<ActionResult<bool>> Logout() =>
            Generate.ActionResult(await authService.LogoutAsync(HttpContext));

        [HttpGet("VerifyAuth")]
        [Authorize]
        public ActionResult VerifyAuth() => Ok("Authenticated");

        // REMOVE IN PRODUCTION

        [HttpPost("DevLogin")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<AuthResponseDto?>> DevLogin(LoginRequestDto request) =>
            Generate.ActionResult<AuthResponseDto?>(await authService.DevLoginAsync(request, HttpContext));



        [HttpPost("ForgotPassword")]
        [EnableRateLimiting("PasswordResetPolicy")]
        public async Task<ActionResult<string>> ForgotPassword([FromBody] RequestPasswordResetDto request) =>
            Generate.ActionResult(await passwordResetService.ForgotPasswordAsync(request));

        [HttpPost("ResetPassword")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ConfirmPasswordResetDto request) =>
            Generate.ActionResult(await passwordResetService.ResetPasswordAsync(request));

    }
}