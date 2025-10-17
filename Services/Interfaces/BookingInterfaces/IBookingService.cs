using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingService
    {
        Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken);
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId, ClaimsPrincipal claimsPrincipal);
        Task<ServiceResponse<List<BookingResponseDto>>> GetUserBookingsAsync(ClaimsPrincipal claimsPrincipal);
        Task<ServiceResponse<List<BookingResponseDto>>> GetAvailableBookingsAsync();
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, ClaimsPrincipal claimsPrincipal);
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdForGuestAsync(int bookingId, string email);
        Task<ServiceResponse<BookingPreviewResponseDto>> GetBookingPreviewAsync(BookingPreviewRequestDto previewDto);

    }
}