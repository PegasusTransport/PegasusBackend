using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Models;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<User> userManager) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> Login(LoginReguest reguest)
        {
            var user = await userManager.FindByEmailAsync(reguest.Email);
            if (user == null)
            {
                return BadRequest();
            }
            if (user.Email != reguest.Email)
            {
                return BadRequest("Email not found");
            }
            if(!await userManager.CheckPasswordAsync(user, reguest.Password)) 
            {
                return BadRequest("Wrong password");
            }
            return Ok("Logged in");
        }
    }
}
