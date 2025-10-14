using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PegasusBackend.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly UserManager<User> _userManager;
        private readonly IMapService _mapService;
        private readonly IPriceService _priceService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;
        private readonly IConfiguration _configuration;

        public BookingService(
            IBookingRepo bookingRepo,
            UserManager<User> userManager,
            IMapService mapService,
            IPriceService priceService,
            IEmailService emailService,
            ILogger<BookingService> logger,
            IConfiguration configuration)
        {
            _bookingRepo = bookingRepo;
            _userManager = userManager;
            _mapService = mapService;
            _priceService = priceService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            try
            {
                var validationResult = await ValidateBookingAsync(bookingDto);
                if (!validationResult.IsValid)
                    return validationResult.ErrorResponse!;

                var user = await _userManager.FindByEmailAsync(bookingDto.Email);
                var isGuestBooking = user == null;

                var booking = CreateBookingEntity(
                    bookingDto,
                    validationResult.RouteInfo!,
                    validationResult.CalculatedPrice,
                    user,
                    isGuestBooking);

                var createdBooking = await _bookingRepo.CreateBookingAsync(booking);

                await SendBookingEmailAsync(createdBooking, bookingDto, isGuestBooking);

                return BuildSuccessResponse(createdBooking, isGuestBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for email: {Email}", bookingDto.Email);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while creating the booking."
                );
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(confirmationToken))
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid confirmation token."
                    );
                }

                var booking = await _bookingRepo.GetBookingByConfirmationTokenAsync(confirmationToken);

                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found."
                    );
                }

                if (booking.ConfirmationTokenExpiresAt < DateTime.UtcNow)
                {
                    await _bookingRepo.DeleteBookingAsync(booking.BookingId);
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Confirmation token has expired. The booking has been removed. Please create a new booking."
                    );
                }

                if (booking.IsConfirmed)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Booking is already confirmed."
                    );
                }

                booking.Status = BookingStatus.Confirmed;
                booking.IsConfirmed = true;
                booking.IsAvailable = true;
                booking.ConfirmationToken = null;
                booking.ConfirmationTokenExpiresAt = null;

                await _bookingRepo.UpdateBookingAsync(booking);

                var response = MapToResponseDTO(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Booking confirmed successfully!"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking with token: {Token}", confirmationToken);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while confirming the booking."
                );
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId, ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var user = await _userManager.GetUserAsync(claimsPrincipal);
                if (user == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "User not found."
                    );
                }

                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found."
                    );
                }

                if (booking.UserIdFk != user.Id)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "You don't have permission to view this booking."
                    );
                }

                var response = MapToResponseDTO(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Booking retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", bookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong."
                );
            }
        }

        public async Task<ServiceResponse<List<BookingResponseDto>>> GetUserBookingsAsync(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var user = await _userManager.GetUserAsync(claimsPrincipal);
                if (user == null)
                {
                    return ServiceResponse<List<BookingResponseDto>>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "User not found."
                    );
                }

                var bookings = await _bookingRepo.GetUserBookingsAsync(user.Id);
                var response = bookings.Select(b => MapToResponseDTO(b)).ToList();

                return ServiceResponse<List<BookingResponseDto>>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Bookings retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user", ex);
                return ServiceResponse<List<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong."
                );
            }
        }

        public async Task<ServiceResponse<List<BookingResponseDto>>> GetAvailableBookingsAsync()
        {
            try
            {
                var bookings = await _bookingRepo.GetAvailableBookingsAsync();
                var response = bookings.Select(b => MapToResponseDTO(b)).ToList();

                return ServiceResponse<List<BookingResponseDto>>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Available bookings retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available bookings");
                return ServiceResponse<List<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong."
                );
            }
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var user = await _userManager.GetUserAsync(claimsPrincipal);
                if (user == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "User not found."
                    );
                }

                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found."
                    );
                }

                if (booking.UserIdFk != user.Id)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "You don't have permission to cancel this booking."
                    );
                }

                booking.Status = BookingStatus.Cancelled;
                booking.IsAvailable = false;

                await _bookingRepo.UpdateBookingAsync(booking);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Booking cancelled successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong."
                );
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> GetBookingByIdForGuestAsync(int bookingId, string email)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found."
                    );
                }

                if (booking.UserIdFk != null || booking.GuestEmail?.ToLower() != email.ToLower())
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "You don't have permission to view this booking."
                    );
                }

                var response = MapToResponseDTO(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Booking retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guest booking {BookingId}", bookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong."
                );
            }
        }

        #region Private Helper Methods - Validation

        private async Task<ValidationResult> ValidateBookingAsync(CreateBookingDto bookingDto)
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

        private ServiceResponse<BookingResponseDto>? ValidatePickupTime(DateTime pickUpDateTime)
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

        private async Task<RouteValidationResult> VerifyRouteAsync(CreateBookingDto bookingDto)
        {
            var coordinates = BuildCoordinatesList(bookingDto);
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

        private ServiceResponse<BookingResponseDto>? ValidateArlandaRequirements(CreateBookingDto bookingDto)
        {
            var allAddresses = GetAllAddresses(bookingDto);
            var hasArlanda = allAddresses.Any(a => a.Contains("arlanda", StringComparison.OrdinalIgnoreCase));

            if (hasArlanda &&
                bookingDto.PickUpAddress.Contains("arlanda", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(bookingDto.Flightnumber))
            {
                _logger.LogWarning("Arlanda validation failed - no flight number");
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Flight number is required for trips departing from Arlanda."
                );
            }

            return null;
        }

        private async Task<PriceValidationResult> CalculateAndVerifyPriceAsync(CreateBookingDto bookingDto, RouteInfoDto routeInfo)
        {
            var priceRequest = BuildPriceCalculationRequest(bookingDto, routeInfo);
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

            if (bookingDto.ExpectedPrice > 0)
            {
                var priceDifference = Math.Abs(priceResponse.Data - bookingDto.ExpectedPrice);
                if (priceDifference > 5)
                {
                    return new PriceValidationResult
                    {
                        IsValid = false,
                        ErrorResponse = ServiceResponse<BookingResponseDto>.FailResponse(
                            HttpStatusCode.BadRequest,
                            $"Price mismatch. Calculated: {priceResponse.Data:F2} SEK, Expected: {bookingDto.ExpectedPrice:F2} SEK. Please refresh and try again."
                        )
                    };
                }
            }

            return new PriceValidationResult { IsValid = true, Price = priceResponse.Data };
        }

        #endregion

        #region Private Helper Methods - Building & Mapping

        private Bookings CreateBookingEntity(
            CreateBookingDto bookingDto,
            RouteInfoDto routeInfo,
            decimal calculatedPrice,
            User? user,
            bool isGuestBooking)
        {
            return new Bookings
            {
                UserIdFk = user?.Id,
                GuestEmail = isGuestBooking ? bookingDto.Email : null,
                GuestFirstName = isGuestBooking ? bookingDto.FirstName : null,
                GuestLastName = isGuestBooking ? bookingDto.LastName : null,
                GuestPhoneNumber = isGuestBooking ? bookingDto.PhoneNumber : null,
                Price = calculatedPrice,
                BookingDateTime = DateTime.UtcNow,
                PickUpDateTime = bookingDto.PickUpDateTime,
                PickUpAdress = bookingDto.PickUpAddress,
                PickUpLatitude = bookingDto.PickUpLatitude,
                PickUpLongitude = bookingDto.PickUpLongitude,
                FirstStopAddress = bookingDto.FirstStopAddress,
                FirstStopLatitude = bookingDto.FirstStopLatitude,
                FirstStopLongitude = bookingDto.FirstStopLongitude,
                SecondStopAddress = bookingDto.SecondStopAddress,
                SecondStopLatitude = bookingDto.SecondStopLatitude,
                SecondStopLongitude = bookingDto.SecondStopLongitude,
                DropOffAdress = bookingDto.DropOffAddress,
                DropOffLatitude = bookingDto.DropOffLatitude,
                DropOffLongitude = bookingDto.DropOffLongitude,
                DistanceKm = routeInfo.DistanceKm,
                DurationMinutes = routeInfo.DurationMinutes,
                Flightnumber = bookingDto.Flightnumber,
                Comment = bookingDto.Comment,
                Status = isGuestBooking ? BookingStatus.PendingEmailConfirmation : BookingStatus.Confirmed,
                ConfirmationToken = isGuestBooking ? GenerateConfirmationToken() : null,
                ConfirmationTokenExpiresAt = isGuestBooking ? DateTime.UtcNow.AddHours(24) : null,
                IsConfirmed = !isGuestBooking,
                IsAvailable = !isGuestBooking
            };
        }

        private List<CoordinateDto> BuildCoordinatesList(CreateBookingDto dto)
        {
            var coordinates = new List<CoordinateDto>
            {
                new() { Latitude = (decimal)dto.PickUpLatitude, Longitude = (decimal)dto.PickUpLongitude }
            };

            if (!string.IsNullOrEmpty(dto.FirstStopAddress) && dto.FirstStopLatitude.HasValue && dto.FirstStopLongitude.HasValue)
            {
                coordinates.Add(new() { Latitude = (decimal)dto.FirstStopLatitude.Value, Longitude = (decimal)dto.FirstStopLongitude.Value });
            }

            if (!string.IsNullOrEmpty(dto.SecondStopAddress) && dto.SecondStopLatitude.HasValue && dto.SecondStopLongitude.HasValue)
            {
                coordinates.Add(new() { Latitude = (decimal)dto.SecondStopLatitude.Value, Longitude = (decimal)dto.SecondStopLongitude.Value });
            }

            coordinates.Add(new() { Latitude = (decimal)dto.DropOffLatitude, Longitude = (decimal)dto.DropOffLongitude });

            return coordinates;
        }

        private PriceCalculationRequestDto BuildPriceCalculationRequest(CreateBookingDto dto, RouteInfoDto routeInfo)
        {
            var request = new PriceCalculationRequestDto
            {
                PickupAdress = dto.PickUpAddress,
                DropoffAdress = dto.DropOffAddress,
                LastDistanceKm = 0,
                LastDurationMinutes = 0
            };

            if (routeInfo.Sections == null || routeInfo.Sections.Count == 0)
                return request;

            if (routeInfo.Sections.Count == 1)
            {
                request.LastDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[0].DurationMinutes;
            }
            else if (routeInfo.Sections.Count == 2)
            {
                request.FirstStopAdress = dto.FirstStopAddress;
                request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                request.LastDistanceKm = routeInfo.Sections[1].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[1].DurationMinutes;
            }
            else if (routeInfo.Sections.Count >= 3)
            {
                request.FirstStopAdress = dto.FirstStopAddress;
                request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                request.SecondStopAdress = dto.SecondStopAddress;
                request.SecondStopDistanceKm = routeInfo.Sections[1].DistanceKm;
                request.SecondStopDurationMinutes = routeInfo.Sections[1].DurationMinutes;
                request.LastDistanceKm = routeInfo.Sections[2].DistanceKm;
                request.LastDurationMinutes = routeInfo.Sections[2].DurationMinutes;
            }

            return request;
        }

        private BookingResponseDto MapToResponseDTO(Bookings booking)
        {
            bool isGuestBooking = booking.UserIdFk == null;

            string email, firstName, lastName, phoneNumber;

            if (isGuestBooking)
            {
                email = booking.GuestEmail!;
                firstName = booking.GuestFirstName!;
                lastName = booking.GuestLastName!;
                phoneNumber = booking.GuestPhoneNumber!;
            }
            else
            {
                email = booking.User!.Email!;
                firstName = booking.User.FirstName;
                lastName = booking.User.LastName;
                phoneNumber = booking.User.PhoneNumber!;
            }

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                IsGuestBooking = isGuestBooking,
                Price = booking.Price,
                BookingDateTime = booking.BookingDateTime,
                PickUpDateTime = booking.PickUpDateTime,
                PickUpAddress = booking.PickUpAdress,
                PickUpLatitude = booking.PickUpLatitude,
                PickUpLongitude = booking.PickUpLongitude,
                FirstStopAddress = booking.FirstStopAddress,
                FirstStopLatitude = booking.FirstStopLatitude,
                FirstStopLongitude = booking.FirstStopLongitude,
                SecondStopAddress = booking.SecondStopAddress,
                SecondStopLatitude = booking.SecondStopLatitude,
                SecondStopLongitude = booking.SecondStopLongitude,
                DropOffAddress = booking.DropOffAdress,
                DropOffLatitude = booking.DropOffLatitude,
                DropOffLongitude = booking.DropOffLongitude,
                DistanceKm = booking.DistanceKm,
                DurationMinutes = booking.DurationMinutes,
                Flightnumber = booking.Flightnumber,
                Comment = booking.Comment,
                Status = booking.Status,
                IsConfirmed = booking.IsConfirmed,
                DriverId = booking.DriverIdFK
            };
        }

        #endregion

        #region Private Helper Methods - Email & Response

        private async Task SendBookingEmailAsync(Bookings booking, CreateBookingDto bookingDto, bool isGuestBooking)
        {
            if (isGuestBooking)
            {
                await _emailService.SendGuestConfirmationEmailAsync(
                    bookingDto.Email,
                    bookingDto.FirstName,
                    booking.ConfirmationToken!,
                    booking
                );
            }
            else
            {
                await _emailService.SendRegisteredUserBookingEmailAsync(
                    bookingDto.Email,
                    bookingDto.FirstName,
                    booking
                );
            }
        }

        private ServiceResponse<BookingResponseDto> BuildSuccessResponse(Bookings booking, bool isGuestBooking)
        {
            var response = MapToResponseDTO(booking);
            string message = isGuestBooking
                ? "Booking created successfully. Please check your email to confirm."
                : "Booking confirmed successfully!";

            return ServiceResponse<BookingResponseDto>.SuccessResponse(
                HttpStatusCode.OK,
                response,
                message
            );
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

        private string GenerateConfirmationToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }

        #endregion

        #region Validation Result Classes

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public RouteInfoDto? RouteInfo { get; set; }
            public decimal CalculatedPrice { get; set; }
            public ServiceResponse<BookingResponseDto>? ErrorResponse { get; set; }
        }

        private class RouteValidationResult
        {
            public bool IsValid { get; set; }
            public RouteInfoDto? RouteInfo { get; set; }
            public ServiceResponse<BookingResponseDto>? ErrorResponse { get; set; }
        }

        private class PriceValidationResult
        {
            public bool IsValid { get; set; }
            public decimal Price { get; set; }
            public ServiceResponse<BookingResponseDto>? ErrorResponse { get; set; }
        }

        #endregion
    }
}