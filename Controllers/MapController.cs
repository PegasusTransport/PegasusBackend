using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Helpers.StatusMapper;
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
        public async Task<ActionResult<RouteInfoDto>> GetRouteInfo([FromBody] List<CoordinateDto> coordinates) =>
            Generate.ActionResult(await _mapService.GetRouteInfoAsync(coordinates));

        [HttpPost("location-info")]
        public async Task<ActionResult<LocationInfoDto>> GetLocationInfo([FromBody] CoordinateDto coordinate) =>
             Generate.ActionResult(await _mapService.GetLocationDetailsAsync(coordinate));
    }
}
