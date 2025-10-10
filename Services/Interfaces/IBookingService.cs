
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken);
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId, string userId);
        Task<ServiceResponse<List<BookingResponseDto>>> GetUserBookingsAsync(string userId);
        Task<ServiceResponse<List<BookingResponseDto>>> GetAvailableBookingsAsync();
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, string userId);
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdForGuestAsync(int bookingId, string email);
    }
}