using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Models;
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
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<CreatedDriverDTO>> CreateDriver(CreateDriverDTO request) =>
            Generate.ActionResult(await driverService.CreateDriverAsync(request, HttpContext));

        [HttpGet("GetAllDrivers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AllDriversDTO>>> GetAllDriver() =>
            Generate.ActionResult(await driverService.GetAllDriversAsync());

        [HttpGet("GetDriverById/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverDTO>> GetDriverById(Guid id) =>
            Generate.ActionResult(await driverService.GetDriverByIdAsync(id));

        [HttpGet("GetDriverByUserId")]
        [Authorize]
        public async Task<ActionResult<DriverDTO>> GetDriverByUserId() =>
            Generate.ActionResult(await driverService.GetDriverByUserIdAsync(HttpContext));

        [HttpPut("UpdateDriver/{id}")]
        [Authorize(Roles = "Admin,Driver")]
        public async Task<ActionResult<UpdateDriverResponseDTO>> UpdateDriver(Guid id, UpdateDriverDTO request) =>
            Generate.ActionResult(await driverService.UpdateDriverAsync(id, request, HttpContext));

        [HttpDelete("DeleteDriver/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> DeleteDriver(Guid id) =>
            Generate.ActionResult(await driverService.DeleteDriverAsync(id));

    }
}
