using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController(UserManager<User> _userManager, ICarService carService) : ControllerBase
    {
        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok("Hello from Pegasus 🚖");
        }

        [HttpGet("testauth")]
        [Authorize]
        public ActionResult AuthTest()
        {
            return Ok("Authorizad");
        }

        [HttpGet("testDriver")]
        [Authorize(Roles ="Driver")]
        public ActionResult DriverTest()
        {
            return Ok("Driver");
        }

        [HttpGet("Admin")]
        [Authorize(Roles ="Admin")]
        public ActionResult AdminTest()
        {
            return Ok("You are admin");
        }

        [HttpGet("check-roles")]
        [Authorize]
        public async Task<ActionResult> CheckMyRoles()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user!);
            return Ok(new { user!.Email, Roles = roles });
        }
        [HttpGet("GetCarinfo")]
        public async Task<ActionResult> GetCarinfo(string regNo)
        {
            var result = await carService.CreateOrFindCar(regNo);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
