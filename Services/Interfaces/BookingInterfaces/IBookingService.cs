using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Responses;
using System.Security.Claims;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingService
    {
        Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<ServiceResponse<BookingPreviewResponseDto>> GetBookingPreviewAsync(BookingPreviewRequestDto previewDto);
        Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken);
        Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetMyBookingsAsync(BookingSearchRequestDto query);
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingAsync(UpdateBookingDto updateDto);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);

    }
}