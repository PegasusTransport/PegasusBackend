using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Models;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController(UserManager<User> _userManager) : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Hello from Pegasus 🚖");
        }
        [HttpGet("testauth")]
        [Authorize]
        public IActionResult AuthTest()
        {
            return Ok("Authorizad");
        }
        [HttpGet("testDriver")]
        [Authorize(Roles ="Driver")]
        public IActionResult DriverTest()
        {
            return Ok("Driver");
        }
        [HttpGet("check-roles")]
        [Authorize]
        public async Task<IActionResult> CheckMyRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return Ok(new { user.Email, Roles = roles });
        }
    }
}
