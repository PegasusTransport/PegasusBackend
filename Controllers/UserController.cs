using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509.Qualified;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers;
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
            Generate.ActionResult(await userService.RegisterUser(request)); 
    }
}
