using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Models;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("GetAllTaxiPrices")]
        public async Task<ActionResult<TaxiSettings>> GetTaxiPrices() =>
            Generate.ActionResult(await _adminService.GetTaxiPricesAsync());


        [HttpPost("CreateNewTaxiPrices")]
        public async Task<ActionResult<TaxiSettings>> CreatNewTaxiPrices(NewTaxiSettingsDTO taxiSettingsDTO) =>
            Generate.ActionResult(await _adminService.CreatePricesAsync(taxiSettingsDTO));
 
    }
}
