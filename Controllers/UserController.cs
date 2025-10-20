using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Org.BouncyCastle.Asn1.X509.Qualified;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost("Registration")]
        [EnableRateLimiting("RegistrationPolicy")]
        public async Task<ActionResult<RegistrationResponseDto>> RegisterUser(RegistrationRequestDto request) => 
            Generate.ActionResult(await userService.RegisterUserAsync(request, $"{Request.Scheme}://{Request.Host}"));

        [HttpGet("ConfirmMail")]
        public async Task<ActionResult<string>> ConfirmMail() => 
            Generate.ActionResult(await userService.)


        [HttpGet("GetUser/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseDto>> GetUserByEmail(string email) =>
            Generate.ActionResult(await userService.GetUserByEmail(email));

        [HttpGet("GetLoggedInUserData")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetLoggedInUserData() =>
            Generate.ActionResult(await userService.GetLoggedInUser(HttpContext));

        [HttpPut("UpdateUser")]
        [Authorize]
        public async Task<ActionResult<UpdateUserResponseDto>> UpdateUser(UpdateUserRequestDto request) =>
            Generate.ActionResult(await userService.UpdateUserAsync(request, HttpContext));
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AllUserResponseDto>>> GetAllUsers() =>
            Generate.ActionResult(await userService.GetAllUsers());
        [HttpDelete("DeleteUser")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteUser() =>
            Generate.ActionResult(await userService.DeleteUserAsync(HttpContext));
    }
}
