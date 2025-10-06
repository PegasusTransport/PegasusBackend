using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.AuthDTOs;
using System.Net;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login(LoginRequestDTO request)
    {
        var response = await _authService.LoginAsync(request, HttpContext);

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