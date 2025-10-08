using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        [HttpPost("route-info")]
        public async Task<ActionResult> GetRouteInfo([FromBody] List<CoordinateDto> coordinates)
        {
            var result = await _mapService.GetRouteInfoAsync(coordinates);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("location-info")]
        public async Task<ActionResult> GetLocationInfo([FromBody] CoordinateDto coordinate)
        {
            var result = await _mapService.GetLocationDetailsAsync(coordinate);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
