using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController(IDriverService driverService) : ControllerBase
    {
        [HttpPost("CreateDriver")]
        [Authorize]
        public async Task<ActionResult<CreatedDriverDTO>> CreateDriver(CreateDriverDTO request) =>
            Generate.ActionResult(await driverService.CreateDriverAsync(request, HttpContext));

        [HttpGet("GetAllDrivers")]
        [Authorize]
        public async Task<ActionResult<List<AllDriversDTO>>> GetAllDriver() =>
            Generate.ActionResult(await driverService.GetAllDriversAsync());


        
    }
}
