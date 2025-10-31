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
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingAsync(UpdateBookingDto updateDto, string? token = null, ClaimsPrincipal? user = null);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);

        // this one is specially for frontend to be able to prefill the form for guest users who wants
        // to update their booking!
        Task<ServiceResponse<BookingResponseDto>> GetBookingByTokenAsync(string token); 
    }
}