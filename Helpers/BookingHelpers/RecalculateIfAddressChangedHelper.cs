using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Helpers.BookingHelpers
{
    public class RecalculateIfAddressChangedHelper
    {
        private readonly IBookingValidationService _validationService;

        public RecalculateIfAddressChangedHelper(IBookingValidationService validationService)
        {
            _validationService = validationService;
        }

        public async Task<ServiceResponse<BookingResponseDto>?> RecalculateIfAddressChangedAsync(Bookings booking, UpdateBookingDto dto)
        {
            bool addressChanged =
                booking.PickUpAdress != dto.PickUpAddress ||
                booking.FirstStopAddress != dto.FirstStopAddress ||
                booking.SecondStopAddress != dto.SecondStopAddress ||
                booking.DropOffAdress != dto.DropOffAddress;

            if (!addressChanged)
                return null;

            var tempDto = new CreateBookingDto
            {
                PickUpDateTime = dto.PickUpDateTime,
                PickUpAddress = dto.PickUpAddress,
                PickUpLatitude = dto.PickUpLatitude,
                PickUpLongitude = dto.PickUpLongitude,
                FirstStopAddress = dto.FirstStopAddress,
                FirstStopLatitude = dto.FirstStopLatitude,
                FirstStopLongitude = dto.FirstStopLongitude,
                SecondStopAddress = dto.SecondStopAddress,
                SecondStopLatitude = dto.SecondStopLatitude,
                SecondStopLongitude = dto.SecondStopLongitude,
                DropOffAddress = dto.DropOffAddress,
                DropOffLatitude = dto.DropOffLatitude,
                DropOffLongitude = dto.DropOffLongitude
            };

            var arlandaValidation = _validationService.ValidateArlandaRequirements(tempDto);
            if (arlandaValidation.StatusCode != HttpStatusCode.OK)
            {
                return ServiceResponse<BookingResponseDto>.FailResponse(arlandaValidation.StatusCode, arlandaValidation.Message);
            }

            var route = await _validationService.VerifyRouteAsync(tempDto);
            if (!route.IsValid)
                return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Could not verify the new route. Please check addresses.");

            var price = await _validationService.CalculateAndVerifyPriceAsync(tempDto, route.RouteInfo!);
            if (!price.IsValid)
                return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Could not calculate a price for the new route.");

            booking.DistanceKm = route.RouteInfo.DistanceKm;
            booking.DurationMinutes = route.RouteInfo.DurationMinutes;
            booking.Price = price.Price;

            return null;
        }
    }
}
