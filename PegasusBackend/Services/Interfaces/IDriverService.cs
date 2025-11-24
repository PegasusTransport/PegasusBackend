using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces
{
    public interface IDriverService
    {
        // This should also be for Admin!
        Task<ServiceResponse<bool>> CreateDriverAsync(CreateRequestDriverDto driver);
        Task<ServiceResponse<UpdateDriverResponseDto>> UpdateDriverAsync(Guid driverId, Drivers updatedDriver, HttpContext httpContext);

        //// Booking features
        Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetDriverBookingsAsync([FromBody] BookingSearchRequestDto query);
        Task<ServiceResponse<BookingResponseDto>> AcceptBookingAsync(int bookingId);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);
        Task<ServiceResponse<bool>> ReassignBookingAsync(int bookingId, Guid newDriverId);
        Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAvailableBookingsAsync(BookingSearchRequestDto query);
        Task<ServiceResponse<BookingResponseDto>> CompleteBookingAsync(int bookingId);
        Task<ServiceResponse<bool>> SendReceiptAsync(ReceiptRequestDto receiptDto);
        Task<ServiceResponse<DriverResponseDto>> GetCurrentDriverAsync();

    }
}
