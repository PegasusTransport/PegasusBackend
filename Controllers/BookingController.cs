using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces;
using System.Collections.Generic;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly UserManager<User> _userManager;

        public BookingController(IBookingService bookingService, UserManager<User> userManager)
        {
            _bookingService = bookingService;
            _userManager = userManager;
        }


        /// Create a new booking (for both guests and registered users)
        [HttpPost("create")]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            var response = await _bookingService.CreateBookingAsync(bookingDto);
            return Generate.ActionResult(response);
        }

        /// Confirm a booking via email token (guests only)
        [HttpGet("confirm")]
        public async Task<ActionResult<BookingResponseDto>> ConfirmBooking([FromQuery] string token)
        {
            var response = await _bookingService.ConfirmBookingAsync(token);
            return Generate.ActionResult(response);
        }


        /// Get a specific booking (registered users only)
        [HttpGet("{bookingId}")]
        [Authorize]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            var response = await _bookingService.GetBookingByIdAsync(bookingId, user.Id);
            return Generate.ActionResult(response);
        }

        /// Get a specific booking for guest (using email)
        [HttpGet("guest/{bookingId}")]
        public async Task<ActionResult<BookingResponseDto>> GetGuestBooking(int bookingId, [FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Email is required." });

            var response = await _bookingService.GetBookingByIdForGuestAsync(bookingId, email);
            return Generate.ActionResult(response);
        }

        /// Get all bookings for the authenticated user
        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            var response = await _bookingService.GetUserBookingsAsync(user.Id);
            return Generate.ActionResult(response);
        }


        /// Get all available bookings (for drivers/admins)
        [HttpGet("available")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetAvailableBookings()
        {
            var response = await _bookingService.GetAvailableBookingsAsync();
            return Generate.ActionResult(response);
        }


        /// Cancel a booking (registered users only)
        [HttpDelete("{bookingId}/cancel")]
        [Authorize]
        public async Task<ActionResult<bool>> CancelBooking(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            var response = await _bookingService.CancelBookingAsync(bookingId, user.Id);
            return Generate.ActionResult(response);
        }
    }
}
