using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginReguest reguest)
        {
            var result = await authService.LoginAsync(reguest);

            if (!result.Success)
            {
                return BadRequest(result.Message);
               
            }
            return Ok(result.Data);
        }
    }
}
