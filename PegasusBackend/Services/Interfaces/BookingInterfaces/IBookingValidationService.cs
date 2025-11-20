using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.ValidationResults;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingValidationService
    {
        Task<ValidationResult> ValidateBookingAsync(CreateBookingDto bookingDto);
        Task<ServiceResponse<BookingResponseDto>> ValidatePickupTimeAsync(DateTime pickUpDateTime, int hours);
        Task<RouteValidationResult> VerifyRouteAsync(CreateBookingDto bookingDto);
        ServiceResponse<BookingResponseDto> ValidateArlandaRequirements(CreateBookingDto bookingDto);
        Task<PriceValidationResult> CalculateAndVerifyPriceAsync(CreateBookingDto bookingDto, RouteInfoDto routeInfo);
    }
}
