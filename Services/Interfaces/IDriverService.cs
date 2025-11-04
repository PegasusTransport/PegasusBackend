using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces
{
    public interface IDriverService
    {
        // Admin Feet
        Task<ServiceResponse<List<AllDriversRequestDto>>> GetAllDriversAsync();
        Task<ServiceResponse<DriverResponseDto>> GetDriverByIdAsync(Guid driverId);
        Task<ServiceResponse<DriverResponseDto>> GetDriverByUserIdAsync(string userId);
        Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId);

        // This should also be for Admin!
        Task<ServiceResponse<CreatedResponseDriverDto>> CreateDriverAsync(CreateRequestDriverDto driver, HttpContext httpContext);
        Task<ServiceResponse<UpdateDriverResponseDto>> UpdateDriverAsync(Guid driverId, UpdateRequestDriverDto updatedDriver, HttpContext httpContext);

        //// Booking features
        Task<ServiceResponse<List<BookingResponseDto>>> GetDriverBookingsAsync(string driverId, string? statusFilter);
        Task<ServiceResponse<BookingResponseDto>> AcceptBookingAsync(int bookingId, string driverId);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, string driverId);
        Task<ServiceResponse<bool>> ReassignBookingAsync();
        Task<ServiceResponse<List<BookingResponseDto>>> GetAvailableBookingsAsync(BookingFilterRequestForAdminDto? filters);
        Task<ServiceResponse<BookingResponseDto>> CompleteBookingAsync(int bookingId, string driverId);
        Task<ServiceResponse<bool>> SendReceiptAsync(ReceiptRequestDto receiptDto, string driverId);
    }
}
