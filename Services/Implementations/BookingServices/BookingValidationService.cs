using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.ValidationResults;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations.BookingServices
{
    public class BookingValidationService : IBookingValidationService
    {
        private readonly IMapService _mapService;
        private readonly IPriceService _priceService;
        private readonly IBookingFactoryService _bookingFactory;
        private readonly ILogger<BookingValidationService> _logger;

        public BookingValidationService(
            IMapService mapService,
            IPriceService priceService,
            IBookingFactoryService bookingFactory,
            ILogger<BookingValidationService> logger)
        {
            _mapService = mapService;
            _priceService = priceService;
            _bookingFactory = bookingFactory;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateBookingAsync(CreateBookingDto bookingDto)
        {
            var pickupValidation = ValidatePickupTime(bookingDto.PickUpDateTime);
            if (pickupValidation != null)
                return new ValidationResult { IsValid = false, ErrorResponse = pickupValidation };

            var routeResult = await VerifyRouteAsync(bookingDto);
            if (!routeResult.IsValid)
                return new ValidationResult { IsValid = false, ErrorResponse = routeResult.ErrorResponse };

            var arlandaValidation = ValidateArlandaRequirements(bookingDto);
            if (arlandaValidation != null)
                return new ValidationResult { IsValid = false, ErrorResponse = arlandaValidation };

            var priceResult = await CalculateAndVerifyPriceAsync(bookingDto, routeResult.RouteInfo!);
            if (!priceResult.IsValid)
                return new ValidationResult { IsValid = false, ErrorResponse = priceResult.ErrorResponse };

            return new ValidationResult
            {
                IsValid = true,
                RouteInfo = routeResult.RouteInfo,
                CalculatedPrice = priceResult.Price
            };
        }

        public ServiceResponse<BookingResponseDto>? ValidatePickupTime(DateTime pickUpDateTime)
        {
            if (pickUpDateTime < DateTime.UtcNow.AddHours(48))
            {
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "PickUpDateTime must be at least 48 hours from now."
                );
            }
            return null;
        }

        public async Task<RouteValidationResult> VerifyRouteAsync(CreateBookingDto bookingDto)
        {
            var coordinates = _bookingFactory.BuildCoordinatesList(bookingDto);
            var routeResponse = await _mapService.GetRouteInfoAsync(coordinates);

            if (routeResponse.StatusCode != HttpStatusCode.OK || routeResponse.Data == null)
            {
                _logger.LogWarning("MapService failed: {Status}", routeResponse.StatusCode);
                return new RouteValidationResult
                {
                    IsValid = false,
                    ErrorResponse = ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Could not verify route. Please check addresses and coordinates."
                    )
                };
            }

            _logger.LogInformation("MapService OK - Route verified");
            return new RouteValidationResult { IsValid = true, RouteInfo = routeResponse.Data };
        }

        public ServiceResponse<BookingResponseDto>? ValidateArlandaRequirements(CreateBookingDto bookingDto)
        {
            var allAddresses = GetAllAddresses(bookingDto);
            var hasArlanda = allAddresses.Any(a => a.Contains("arlanda", StringComparison.OrdinalIgnoreCase));

            // At least 1 stop MUST be from Arlanda
            if (!hasArlanda)
            {
                _logger.LogWarning("Arlanda validation failed - no Arlanda address found");
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Minst en av adresserna (pickup, dropoff, eller stopp) måste vara Arlanda."
                );
            }

            // If pickup is from Arlanda, then flightnumber is required.
            if (bookingDto.PickUpAddress.Contains("arlanda", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(bookingDto.Flightnumber))
            {
                _logger.LogWarning("Arlanda validation failed - no flight number for pickup from Arlanda");
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Flygnummer krävs för resor som hämtas upp från Arlanda."
                );
            }

            _logger.LogInformation("Arlanda validation passed");
            return null;
        }

        public async Task<PriceValidationResult> CalculateAndVerifyPriceAsync(CreateBookingDto bookingDto, RouteInfoDto routeInfo)
        {
            var priceRequest = _bookingFactory.BuildPriceCalculationRequest(bookingDto, routeInfo);
            var priceResponse = await _priceService.CalculateTotalPriceAsync(priceRequest);

            if (priceResponse.StatusCode != HttpStatusCode.OK || priceResponse.Data == 0)
            {
                _logger.LogWarning("PriceService failed: {Status}", priceResponse.StatusCode);
                return new PriceValidationResult
                {
                    IsValid = false,
                    ErrorResponse = ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Could not calculate price for the booking."
                    )
                };
            }

            _logger.LogInformation("PriceService OK - Price: {Price}", priceResponse.Data);
            return new PriceValidationResult { IsValid = true, Price = priceResponse.Data };
        }

        private List<string> GetAllAddresses(CreateBookingDto bookingDto)
        {
            var addresses = new List<string> { bookingDto.PickUpAddress };

            if (!string.IsNullOrEmpty(bookingDto.FirstStopAddress))
                addresses.Add(bookingDto.FirstStopAddress);

            if (!string.IsNullOrEmpty(bookingDto.SecondStopAddress))
                addresses.Add(bookingDto.SecondStopAddress);

            addresses.Add(bookingDto.DropOffAddress);

            return addresses;
        }
    }
}