using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs;
namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceController : ControllerBase
    {
        private readonly IPriceService _priceService;

        public PriceController(IPriceService priceService)
        {
            _priceService = priceService;
        }

        [HttpPost("CalculateFullPrice")]
        public async Task<ActionResult<decimal>> CalculateFullPrice([FromBody]PriceCalculationRequestDto priceCalculationRequestDTO )
        {
            var result = await _priceService.CalculateTotalPriceAsync(priceCalculationRequestDTO);

            if (!result.Success)
            {
                return BadRequest("Kunde inte kalkylera priset.");
            }

            return Ok(result);
        }
    }
}
