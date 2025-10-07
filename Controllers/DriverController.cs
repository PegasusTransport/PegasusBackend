using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.Extentions;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController(IDriverService driverService) : ControllerBase
    {
        [HttpPost("CreateDriver")]
        [Authorize]
        public async Task<ActionResult> CreateDriver(CreateDriverDTO request)
        {
            var response = await driverService.CreateDriverAsync(request, HttpContext);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => Ok(response),
                HttpStatusCode.NotFound => NotFound(response),
                HttpStatusCode.BadRequest => BadRequest(response),
                _ => StatusCode((int)response.StatusCode, response)
            };
        }
        [HttpGet("GetAllDrivers")]
        [Authorize]
        public async Task<ActionResult> GetAllDriver()
        {
            var response = await driverService.GetAllDriversAsync();

            return response.StatusCode.IsSuccessStatusCode() ? Ok(response)
                : BadRequest(response);
        }
    }
}
