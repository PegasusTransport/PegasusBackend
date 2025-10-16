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
        public async Task<ActionResult<CreatedResponseDriverDto>> CreateDriver(CreateRequestDriverDto request) =>
            Generate.ActionResult(await driverService.CreateDriverAsync(request, HttpContext));

        [HttpGet("GetAllDrivers")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AllDriversDto>>> GetAllDriver() =>
            Generate.ActionResult(await driverService.GetAllDriversAsync());

        [HttpGet("GetDriverById/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverById(Guid id) =>
            Generate.ActionResult(await driverService.GetDriverByIdAsync(id));

        [HttpGet("GetDriverByUserId/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverByUserId(string id) =>
            Generate.ActionResult(await driverService.GetDriverByUserIdAsync(id));

        [HttpPut("UpdateDriver/{id}")]
        [Authorize(Roles = "Admin,Driver")]
        public async Task<ActionResult<UpdateDriverResponseDto>> UpdateDriver(Guid id, UpdateRequestDriverDto request) =>
            Generate.ActionResult(await driverService.UpdateDriverAsync(id, request, HttpContext));

        [HttpDelete("DeleteDriver/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> DeleteDriver(Guid id) =>
            Generate.ActionResult(await driverService.DeleteDriverAsync(id));

    }
}
