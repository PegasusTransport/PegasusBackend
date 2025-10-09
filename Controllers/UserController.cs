using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<RegistrationResponseDTO>> RegisterUser(RegistrationRequestDTO request) => 
            Generate.ActionResult(await userService.RegisterUserAsync(request));

        [HttpPut("UpdateUser")]
        [Authorize]
        public async Task<ActionResult<UpdateUserResponseDTO>> UpdateUser(UpdateUserRequestDTO request) =>
            Generate.ActionResult(await userService.UpdateUserAsync(request, HttpContext));
        [HttpGet("GetAllUser")]
        public async Task<ActionResult<List<AllUserDTO>>> GetUser() =>
            Generate.ActionResult(await userService.GetAllUsers());
        [HttpDelete("DeleteUser")]
        [Authorize]
        public async Task<ActionResult<bool>> DeleteUser() =>
            Generate.ActionResult(await userService.DeleteUserAsync(HttpContext));
    }
}
