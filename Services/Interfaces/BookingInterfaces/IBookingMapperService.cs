using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingMapperService
    {
        BookingResponseDto MapToResponseDTO(Bookings booking);
        List<BookingResponseDto> MapToResponseDTOs(List<Bookings> bookings);
    }
}