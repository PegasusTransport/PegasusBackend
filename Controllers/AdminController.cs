using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Models;
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
        public async Task<ActionResult<TaxiSettings>> GetTaxiPrices()
        {
            var response =  await _adminService.GetTaxiPricesAsync();

            return response.StatusCode.IsSuccessStatusCode() 
                ? Ok(response) 
                : BadRequest(response);

            //var response = await _adminService.GetTaxiPricesAsync();

            //if (!response.StatusCode.IsSuccessStatusCode())
            //{
            //    return NotFound("Inga priser hittades i databasen!");
            //}

            //return Ok(response.Data);
        }

        [HttpPost("CreateNewTaxiPrices")]
        public async Task<ActionResult<TaxiSettings>> CreatNewTaxiPrices(NewTaxiSettingsDTO taxiSettingsDTO)
        {
            var newSettings = await _adminService.CreatePricesAsync(taxiSettingsDTO);
            
            if (!newSettings.StatusCode.IsSuccessStatusCode())
            {
                return BadRequest("Kunde inte uppdatera Taxameter priset.");
            }

            return Ok(newSettings.Data);
        }


    }
}
