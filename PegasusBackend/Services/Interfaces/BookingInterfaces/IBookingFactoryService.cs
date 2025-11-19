using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingFactoryService
    {
        Bookings CreateBookingEntity(
            CreateBookingDto bookingDto,
            RouteInfoDto routeInfo,
            decimal calculatedPrice,
            User? user,
            bool isGuestBooking);

        List<CoordinateDto> BuildCoordinatesList(CreateBookingDto dto);

        PriceCalculationRequestDto BuildPriceCalculationRequest(CreateBookingDto dto, RouteInfoDto routeInfo);
    }
}