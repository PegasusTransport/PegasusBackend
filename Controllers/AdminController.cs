using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Text.RegularExpressions;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IDriverService _driverService;

        public AdminController(IAdminService adminService, IDriverService driverService)
        {
            _adminService = adminService;
            _driverService = driverService;
        }

        [HttpGet("GetAllTaxiPrices")]
        public async Task<ActionResult<TaxiSettings>> GetTaxiPrices() =>
            Generate.ActionResult(await _adminService.GetTaxiPricesAsync());


        [HttpPost("CreateNewTaxiPrices")]
        public async Task<ActionResult<TaxiSettings>> CreatNewTaxiPrices(NewTaxiSettingsDTO taxiSettingsDTO) =>
            Generate.ActionResult(await _adminService.CreatePricesAsync(taxiSettingsDTO));

        [HttpGet("getAllBookings")]
        public async Task<ActionResult<PaginatedResult<BookingResponseDto>>> GetAllBookings([FromQuery] BookingFilterRequestForAdminDto searchRequestDto) =>
            Generate.ActionResult(await _adminService.GetAllBookingsAsync(searchRequestDto));

        [HttpGet("GetBookingById/{bookingId}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById([FromRoute] int bookingId) =>
            Generate.ActionResult(await _adminService.GetBookingByIdAsync(bookingId));

        [HttpPut("AssignDriver/{bookingId}/{driverId}")]
        public async Task<ActionResult<bool>> AssignDriver([FromRoute] int bookingId, Guid driverId) =>
            Generate.ActionResult(await _adminService.AssignDriverAsync(bookingId, driverId));

        [HttpGet("GetAvailableDrivers/{bookingId}")]
        public async Task<ActionResult<List<AvailableDriverResponsDto>>> GetAvailableDrivers([FromRoute] int bookingId) =>
            Generate.ActionResult(await _adminService.GetAvailbleDrivers(bookingId));

        [HttpPut("CancelBooking/{bookingId}")]
        public async Task<ActionResult<bool>> CancelBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await _adminService.CancelBookingAsync(bookingId));

        [HttpPut("UpdateBooking")]
        public async Task<ActionResult<BookingResponseDto>> UpdateBookingById([FromBody] UpdateBookingDto updateBookingDto) =>
            Generate.ActionResult(await _adminService.ChangeBookingById(updateBookingDto));

        // DriverSection
        [HttpPost("CreateDriver")]
        [EnableRateLimiting("RegistrationPolicy")]
        public async Task<ActionResult<bool>> CreateDriver(CreateRequestDriverDto request) =>
            Generate.ActionResult(await _driverService.CreateDriverAsync(request));

        [HttpGet("GetAllDrivers")]
        public async Task<ActionResult<List<AllDriversRequestDto>>> GetAllDriver() =>
            Generate.ActionResult(await _adminService.GetAllDriversAsync());

        [HttpDelete("DeleteDriver/{id}")]
        public async Task<ActionResult<bool>> DeleteDriver(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id is required.");

            var cleaned = Regex.Replace(id, @"\p{Cf}", string.Empty).Trim();

            if (!Guid.TryParse(cleaned, out var guid))
                return BadRequest(new { id = new[] { "The value is not a valid GUID." } });

            return Generate.ActionResult(await _adminService.DeleteDriverAsync(guid));
        }

        [HttpGet("GetDriverById/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverById(Guid id) =>
            Generate.ActionResult(await _adminService.GetDriverByIdAsync(id));

        [HttpGet("GetDriverByUserId/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverByUserId(string id) =>
            Generate.ActionResult(await _adminService.GetDriverByUserIdAsync(id));

        [HttpPost("CreateAdmin/{email}")]
        [Authorize]
        public async Task<ActionResult<bool>> CreateAdmin(string email) =>
            Generate.ActionResult(await _adminService.CreateAdminAsync(email));
    }
}
