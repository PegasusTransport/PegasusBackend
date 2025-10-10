using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;
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
                // 1. Validate 48h rule
                if (bookingDto.PickUpDateTime < DateTime.UtcNow.AddHours(48))
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "PickUpDateTime must be at least 48 hours from now."
                    );
                }

                // 2. Build coordinates list for MapService
                var coordinates = BuildCoordinatesList(bookingDto);

                // 3. Verify route with MapService
                var routeResponse = await _mapService.GetRouteInfoAsync(coordinates);
                if (routeResponse.StatusCode != HttpStatusCode.OK || routeResponse.Data == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Could not verify route. Please check addresses and coordinates."
                    );
                }

                // 4. Calculate price and verify with frontend price
                var priceRequest = BuildPriceCalculationRequest(bookingDto, routeResponse.Data);
                var priceResponse = await _priceService.CalculateTotalPriceAsync(priceRequest);

                if (priceResponse.StatusCode != HttpStatusCode.OK || priceResponse.Data == 0)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Could not calculate price for the booking."
                    );
                }

                // 5. Verify price matches frontend (tolerance of 5 SEK)
                // Skip verification if expectedPrice is 0 (for testing/preview scenarios)
                if (bookingDto.ExpectedPrice > 0)
                {
                    var priceDifference = Math.Abs(priceResponse.Data - bookingDto.ExpectedPrice);
                    if (priceDifference > 5)
                    {
                        return ServiceResponse<BookingResponseDto>.FailResponse(
                            HttpStatusCode.BadRequest,
                            $"Price mismatch. Calculated: {priceResponse.Data:F2} SEK, Expected: {bookingDto.ExpectedPrice:F2} SEK. Please refresh and try again."
                        );
                    }
                }

                // 6. Check if user exists (registered user booking)
                var user = await _userManager.FindByEmailAsync(bookingDto.Email);
                bool isGuestBooking = user == null;

                // 7. Create booking
                var booking = new Bookings
                {
                    // User or Guest
                    UserIdFk = user?.Id,
                    GuestEmail = isGuestBooking ? bookingDto.Email : null,
                    GuestFirstName = isGuestBooking ? bookingDto.FirstName : null,
                    GuestLastName = isGuestBooking ? bookingDto.LastName : null,
                    GuestPhoneNumber = isGuestBooking ? bookingDto.PhoneNumber : null,

                    // Booking details
                    Price = priceResponse.Data,
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
                    DistanceKm = routeResponse.Data.DistanceKm,
                    DurationMinutes = routeResponse.Data.DurationMinutes,
                    Flightnumber = bookingDto.Flightnumber,
                    Comment = bookingDto.Comment,

                    // Registered users get instant confirmation, guests need email confirmation
                    Status = isGuestBooking ? BookingStatus.PendingEmailConfirmation : BookingStatus.Confirmed,
                    ConfirmationToken = isGuestBooking ? GenerateConfirmationToken() : null,
                    ConfirmationTokenExpiresAt = isGuestBooking ? DateTime.UtcNow.AddHours(24) : null,
                    IsConfirmed = !isGuestBooking,
                    IsAvailable = true
                };

                var createdBooking = await _bookingRepo.CreateBookingAsync(booking);

                // 8. Send appropriate email
                if (isGuestBooking)
                {
                    // Guest: Send confirmation email with token
                    await SendGuestConfirmationEmailAsync(
                        bookingDto.Email,
                        bookingDto.FirstName,
                        createdBooking.ConfirmationToken!,
                        createdBooking
                    );
                }
                else
                {
                    // Registered user: Send booking confirmation (already confirmed)
                    await SendRegisteredUserBookingEmailAsync(
                        bookingDto.Email,
                        bookingDto.FirstName,
                        createdBooking
                    );
                }

                // 9. Map to response DTO
                var response = MapToResponseDTO(createdBooking);

                string message = isGuestBooking
                    ? "Booking created successfully. Please check your email to confirm."
                    : "Booking confirmed successfully!";

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    message
                );
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
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Confirmation token has expired."
                    );
                }

                if (booking.IsConfirmed)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Booking is already confirmed."
                    );
                }

                // Update booking status
                booking.Status = BookingStatus.Confirmed;
                booking.IsConfirmed = true;
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

        public async Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId, string userId)
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

                // Check if user owns this booking
                if (booking.UserIdFk != userId)
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

        public async Task<ServiceResponse<List<BookingResponseDto>>> GetUserBookingsAsync(string userId)
        {
            try
            {
                var bookings = await _bookingRepo.GetUserBookingsAsync(userId);
                var response = bookings.Select(b => MapToResponseDTO(b)).ToList();

                return ServiceResponse<List<BookingResponseDto>>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Bookings retrieved successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
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

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId, string userId)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found."
                    );
                }

                if (booking.UserIdFk != userId)
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

                // Check if this is a guest booking and email matches
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

        // Private helper methods
        private List<CoordinateDto> BuildCoordinatesList(CreateBookingDto dto)
        {
            var coordinates = new List<CoordinateDto>
            {
                new CoordinateDto
                {
                    Latitude = (decimal)dto.PickUpLatitude,
                    Longitude = (decimal)dto.PickUpLongitude
                }
            };

            if (!string.IsNullOrEmpty(dto.FirstStopAddress) && dto.FirstStopLatitude.HasValue && dto.FirstStopLongitude.HasValue)
            {
                coordinates.Add(new CoordinateDto
                {
                    Latitude = (decimal)dto.FirstStopLatitude.Value,
                    Longitude = (decimal)dto.FirstStopLongitude.Value
                });
            }

            if (!string.IsNullOrEmpty(dto.SecondStopAddress) && dto.SecondStopLatitude.HasValue && dto.SecondStopLongitude.HasValue)
            {
                coordinates.Add(new CoordinateDto
                {
                    Latitude = (decimal)dto.SecondStopLatitude.Value,
                    Longitude = (decimal)dto.SecondStopLongitude.Value
                });
            }

            coordinates.Add(new CoordinateDto
            {
                Latitude = (decimal)dto.DropOffLatitude,
                Longitude = (decimal)dto.DropOffLongitude
            });

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

            if (routeInfo.Sections != null && routeInfo.Sections.Count > 0)
            {
                if (routeInfo.Sections.Count == 1)
                {
                    // No stops
                    request.LastDistanceKm = routeInfo.Sections[0].DistanceKm;
                    request.LastDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                }
                else if (routeInfo.Sections.Count == 2)
                {
                    // One stop
                    request.FirstStopAdress = dto.FirstStopAddress;
                    request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                    request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                    request.LastDistanceKm = routeInfo.Sections[1].DistanceKm;
                    request.LastDurationMinutes = routeInfo.Sections[1].DurationMinutes;
                }
                else if (routeInfo.Sections.Count >= 3)
                {
                    // Two stops
                    request.FirstStopAdress = dto.FirstStopAddress;
                    request.FirstStopDistanceKm = routeInfo.Sections[0].DistanceKm;
                    request.FirstStopDurationMinutes = routeInfo.Sections[0].DurationMinutes;
                    request.SecondStopAdress = dto.SecondStopAddress;
                    request.SecondStopDistanceKm = routeInfo.Sections[1].DistanceKm;
                    request.SecondStopDurationMinutes = routeInfo.Sections[1].DurationMinutes;
                    request.LastDistanceKm = routeInfo.Sections[2].DistanceKm;
                    request.LastDurationMinutes = routeInfo.Sections[2].DurationMinutes;
                }
            }

            return request;
        }

        private async Task SendGuestConfirmationEmailAsync(
            string email,
            string firstName,
            string token,
            Bookings booking)
        {
            try
            {
                var confirmationUrl = $"{_configuration["AppSettings:FrontendUrl"]}/confirm-booking?token={token}";

                var subject = "Bekräfta din bokning hos Pegasus Transport";
                var emailContent = $@"
Hej {firstName}!

Tack för din bokning hos Pegasus Transport.

Bokningsdetaljer:
- Upphämtning: {booking.PickUpAdress}
- Avlämning: {booking.DropOffAdress}
- Datum & tid: {booking.PickUpDateTime:yyyy-MM-dd HH:mm}
- Pris: {booking.Price:F2} SEK

Klicka på länken nedan för att bekräfta din bokning:
{confirmationUrl}

Länken är giltig i 24 timmar.

Om du vill se dina bokningar i framtiden kan du registrera ett konto på vår hemsida med denna e-postadress.

Med vänliga hälsningar,
Pegasus Transport
";

                var message = new Message(
                    new[] { ("", email) },
                    emailContent,
                    subject
                );

                await _emailService.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending guest confirmation email to: {Email}", email);
                throw;
            }
        }

        private async Task SendRegisteredUserBookingEmailAsync(
            string email,
            string firstName,
            Bookings booking)
        {
            try
            {
                var subject = "Din bokning är bekräftad - Pegasus Transport";
                var emailContent = $@"
Hej {firstName}!

Din bokning hos Pegasus Transport är nu bekräftad!

Bokningsdetaljer:
- Bokningsnummer: #{booking.BookingId}
- Upphämtning: {booking.PickUpAdress}
- Avlämning: {booking.DropOffAdress}
- Datum & tid: {booking.PickUpDateTime:yyyy-MM-dd HH:mm}
- Pris: {booking.Price:F2} SEK

Du kan se alla dina bokningar genom att logga in på din profil på vår hemsida.

Vi ser fram emot att köra dig!

Med vänliga hälsningar,
Pegasus Transport
";

                var message = new Message(
                    new[] { ("", email) },
                    emailContent,
                    subject
                );

                await _emailService.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking confirmation email to: {Email}", email);
                throw;
            }
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

        private BookingResponseDto MapToResponseDTO(Bookings booking)
        {
            // Determine if guest booking and get customer info
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

        // Overload for when we have DTO and User available (during creation)
        private BookingResponseDto MapToResponseDTO(Bookings booking, CreateBookingDto dto, User? user)
        {
            bool isGuestBooking = user == null;

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
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
    }
}