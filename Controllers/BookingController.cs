using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Security.Claims;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }


        /// Create a new booking (for both guests and registered users)
        [HttpPost("create")]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto bookingDto) =>
            Generate.ActionResult(await _bookingService.CreateBookingAsync(bookingDto));


        /// Confirm a booking via email token (guests only)
        [HttpGet("confirm")]
        public async Task<ActionResult<BookingResponseDto>> ConfirmBooking([FromQuery] string token) =>
            Generate.ActionResult(await _bookingService.ConfirmBookingAsync(token));


        /// Get a specific booking (registered users only)
        [HttpGet("{bookingId}")]
        [Authorize]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int bookingId) =>
            Generate.ActionResult(await _bookingService.GetBookingByIdAsync(bookingId, User));


        /// Get a specific booking for guest (using email)
        [HttpGet("guest/{bookingId}")]
        public async Task<ActionResult<BookingResponseDto>> GetGuestBooking(int bookingId, [FromQuery] string email) =>
            Generate.ActionResult(await _bookingService.GetBookingByIdForGuestAsync(bookingId, email));

        /// Get all bookings for the authenticated user
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings() =>
            Generate.ActionResult(await _bookingService.GetUserBookingsAsync(User));


        /// Get all available bookings (for drivers/admins)
        [HttpGet("available")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAvailableBookings() =>
            Generate.ActionResult(await _bookingService.GetAvailableBookingsAsync());


        /// Cancel a booking (registered users only)
        [HttpDelete("{bookingId}/cancel")]
        [Authorize]
        public async Task<ActionResult<bool>> CancelBooking(int bookingId) =>
            Generate.ActionResult(await _bookingService.CancelBookingAsync(bookingId, User));
    }
}