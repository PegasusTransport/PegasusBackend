using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.ValidationResults;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces.BookingInterfaces
{
    public interface IBookingValidationService
    {
        Task<ValidationResult> ValidateBookingAsync(CreateBookingDto bookingDto);
        ServiceResponse<BookingResponseDto>? ValidatePickupTime(DateTime pickUpDateTime);
        Task<RouteValidationResult> VerifyRouteAsync(CreateBookingDto bookingDto);
        ServiceResponse<BookingResponseDto>? ValidateArlandaRequirements(CreateBookingDto bookingDto);
        Task<PriceValidationResult> CalculateAndVerifyPriceAsync(CreateBookingDto bookingDto, RouteInfoDto routeInfo);
    }
}