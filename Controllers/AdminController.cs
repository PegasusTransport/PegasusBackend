using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpDelete("deleteBooking/{bookingId}")]
        public async Task<ActionResult<bool>> DeleteBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await _adminService.DeleteBookingByIdAsync(bookingId));

        [HttpPut("CancelBooking/{bookingId}")]
        public async Task<ActionResult<bool>> CancelBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await _adminService.CancelBookingAsync(bookingId));

        [HttpPut("UpdateBooking")]
        public async Task<ActionResult<BookingResponseDto>> UpdateBookingById([FromBody] UpdateBookingDto updateBookingDto) =>
            Generate.ActionResult(await _adminService.ChangeBookingById(updateBookingDto));
    }
}
