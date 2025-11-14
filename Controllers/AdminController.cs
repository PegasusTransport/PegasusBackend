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

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IDriverService _driverService;

        public AdminController(IAdminService adminService, IDriverService driverService )
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
        public async Task <ActionResult<PaginatedResult<BookingResponseDto>>> GetAllBookings([FromQuery] BookingFilterRequestForAdminDto searchRequestDto) =>
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
        public async Task<ActionResult<CreatedResponseDriverDto>> CreateDriver(CreateRequestDriverDto request) =>
            Generate.ActionResult(await _driverService.CreateDriverAsync(request, HttpContext));

        [HttpGet("GetAllDrivers")]
        public async Task<ActionResult<List<AllDriversRequestDto>>> GetAllDriver() =>
            Generate.ActionResult(await _adminService.GetAllDriversAsync());

        [HttpDelete("DeleteDriver/{id}")]
        public async Task<ActionResult<bool>> DeleteDriver(Guid id) =>
            Generate.ActionResult(await _adminService.DeleteDriverAsync(id));

        [HttpGet("GetDriverById/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverById(Guid id) =>
            Generate.ActionResult(await _adminService.GetDriverByIdAsync(id));

        [HttpGet("GetDriverByUserId/{id}")]
        [Authorize]
        public async Task<ActionResult<DriverResponseDto>> GetDriverByUserId(string id) =>
            Generate.ActionResult(await _adminService.GetDriverByUserIdAsync(id));
    }
}
