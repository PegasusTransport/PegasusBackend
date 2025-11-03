using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
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
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingForUserAsync(UpdateBookingDto updateDto);

        // This method can be used in driverServcie and AdminService. It only contains the logic for updating a booking! 
        Task<ServiceResponse<BookingResponseDto>> UpdateBookingInternalAsync(Bookings booking, UpdateBookingDto updateDto);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);

    }
}