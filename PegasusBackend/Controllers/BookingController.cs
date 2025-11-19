using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PegasusBackend.Attributes;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Services.Interfaces;
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

        [HttpPost("createBooking")]
        [EnableRateLimiting("BookingPolicy")]
        [Idempotent]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingDto bookingDto) =>
            Generate.ActionResult(await _bookingService.CreateBookingAsync(bookingDto));

        /// In advance Calculate price and route information WITHOUT creating a booking.
        /// Does NOT require customer-data (email, name, phone).
        [HttpPost("previewBookingPrice")]
        public async Task<ActionResult<BookingPreviewResponseDto>> GetBookingPreview(
            [FromBody] BookingPreviewRequestDto previewDto) =>
            Generate.ActionResult(await _bookingService.GetBookingPreviewAsync(previewDto));

        /// Confirm a booking via email token (guests only)
        [HttpGet("confirmBooking")]
        public async Task<ActionResult<BookingResponseDto>> ConfirmBooking([FromQuery] string token) =>
            Generate.ActionResult(await _bookingService.ConfirmBookingAsync(token));

        [HttpGet("getMyBookings")]
        [Authorize]
        public async Task<ActionResult<PaginatedResult<BookingResponseDto>>> GetMyBookings([FromQuery] BookingSearchRequestDto query) => 
            Generate.ActionResult(await _bookingService.GetMyBookingsAsync(query));

        [HttpGet("GetBookingById/{bookingId}")]
        [Authorize]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById([FromRoute] int bookingId) =>
            Generate.ActionResult(await _bookingService.GetBookingByIdAsync(bookingId));

        [HttpPut("updateBooking")]
        [Authorize]
        public async Task<ActionResult<BookingResponseDto>> updateBooking([FromBody] UpdateBookingDto updateBookingDto) =>
            Generate.ActionResult(await _bookingService.UpdateBookingForUserAsync(updateBookingDto));

        [HttpPut("CancelBooking/{bookingId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CancelBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await _bookingService.CancelBookingAsync(bookingId));
    }
}