using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Extentions;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Reflection.Metadata.Ecma335;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Driver")]
    public class DriverController(IDriverService driverService) : ControllerBase
    {
        [HttpPost("CreateDriver")]
        [EnableRateLimiting("RegistrationPolicy")]
        public async Task<ActionResult<CreatedResponseDriverDto>> CreateDriver(CreateRequestDriverDto request) =>
            Generate.ActionResult(await driverService.CreateDriverAsync(request, HttpContext));

        [HttpPut("UpdateDriver/{id}")]
        public async Task<ActionResult<UpdateDriverResponseDto>> UpdateDriver(Guid id, UpdateRequestDriverDto request) =>
            Generate.ActionResult(await driverService.UpdateDriverAsync(id, request, HttpContext));

        [HttpGet("GetMyBookings")]
        public async Task<ActionResult<PaginatedResult<BookingResponseDto>>> GetDriverBookingsAsync([FromBody] BookingSearchRequestDto query) =>
            Generate.ActionResult(await driverService.GetDriverBookingsAsync(query));

        [HttpGet("Bookings/Available")]
        public async Task<ActionResult<PaginatedResult<BookingResponseDto>>> GetAvailableBookings([FromQuery] BookingSearchRequestDto query) =>
            Generate.ActionResult(await driverService.GetAvailableBookingsAsync(query));

        [HttpPost("Bookings/{bookingId:int}/Accept")]
        public async Task<ActionResult<BookingResponseDto>> AcceptBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await driverService.AcceptBookingAsync(bookingId));

        [HttpPost("Bookings/{bookingId:int}/Cancel")]
        public async Task<ActionResult<bool>> CancelBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await driverService.CancelBookingAsync(bookingId));

        [HttpPost("Bookings/{bookingId:int}/Reassign/{newDriverId:guid}")]
        public async Task<ActionResult<bool>> ReassignBooking([FromRoute] int bookingId, [FromRoute] Guid newDriverId) =>
            Generate.ActionResult(await driverService.ReassignBookingAsync(bookingId, newDriverId));

        [HttpPost("Bookings/{bookingId:int}/Complete")]
        public async Task<ActionResult<BookingResponseDto>> CompleteBooking([FromRoute] int bookingId) =>
            Generate.ActionResult(await driverService.CompleteBookingAsync(bookingId));

        [HttpPost("Bookings/SendReceipt")]
        public async Task<ActionResult<bool>> SendReceipt([FromBody] ReceiptRequestDto dto) =>
            Generate.ActionResult(await driverService.SendReceiptAsync(dto));

    }
}
