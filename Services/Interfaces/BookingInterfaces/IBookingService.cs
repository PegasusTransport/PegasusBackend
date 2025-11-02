using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingService
    {
        Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<ServiceResponse<BookingPreviewResponseDto>> GetBookingPreviewAsync(BookingPreviewRequestDto previewDto);
        Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken);

        // my new implemented services!
        Task<ServiceResponse<List<BookingResponseDto>>> GetMyBookingsAsync();
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingAsync(UpdateBookingDto updateDto);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);

    }
}