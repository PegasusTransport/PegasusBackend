using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.BookingHelper;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Globalization;
using System.Net;
using System.Security.Claims;

namespace PegasusBackend.Services.Implementations.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepo _bookingRepo;
        private readonly UserManager<User> _userManager;
        private readonly IMailjetEmailService _mailjetEmailService;
        private readonly IBookingValidationService _validationService;
        private readonly IBookingFactoryService _bookingFactory;
        private readonly IBookingMapperService _bookingMapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepo bookingRepo,
            UserManager<User> userManager,
            IMailjetEmailService mailjetEmailService,
            IBookingValidationService validationService,
            IBookingFactoryService bookingFactory,
            IBookingMapperService bookingMapper,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _userManager = userManager;
            _mailjetEmailService = mailjetEmailService;
            _validationService = validationService;
            _bookingFactory = bookingFactory;
            _bookingMapper = bookingMapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            try
            {
                var validationResult = await _validationService.ValidateBookingAsync(bookingDto);
                if (!validationResult.IsValid)
                    return validationResult.ErrorResponse!;

                var user = await _userManager.FindByEmailAsync(bookingDto.Email);
                var isGuestBooking = user == null;


                var booking = _bookingFactory.CreateBookingEntity(
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

                var response = _bookingMapper.MapToResponseDTO(booking);

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

                var response = _bookingMapper.MapToResponseDTO(booking);

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
                var response = _bookingMapper.MapToResponseDTOs(bookings);

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
                var response = _bookingMapper.MapToResponseDTOs(bookings);

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

                var response = _bookingMapper.MapToResponseDTO(booking);

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

        #region Private Helper Methods

        private async Task SendBookingEmailAsync(Bookings booking, CreateBookingDto bookingDto, bool isGuestBooking)
        {
            var stopsText = BookingMailHelper.FormatStops(bookingDto);
            var formattedTime = BookingMailHelper.FormatDateTime(bookingDto.PickUpDateTime);

            var confirmationLink = $"https://localhost:7161/api/booking/confirm?token={booking.ConfirmationToken}";
            //var confirmationLink = $"https://pegasustransport.se/booking/confirm?token={booking.ConfirmationToken}";

            if (isGuestBooking)
            {

                await _mailjetEmailService.SendEmailAsync(
                    bookingDto.Email,
                    Helpers.MailjetHelpers.MailjetTemplateType.PendingConfirmation,
                    new PendingConfirmationDto
                    {
                        Firstname = bookingDto.FirstName,
                        PickupAddress = bookingDto.PickUpAddress,
                        Stops = stopsText,
                        Destination = bookingDto.DropOffAddress,
                        PickupTime = formattedTime,
                        TotalPrice = booking.Price,
                        ConfirmationLink = confirmationLink,
                    },
                    Helpers.MailjetHelpers.MailjetSubjects.PendingConfirmation
                    );
            }
            else
            {
                await _mailjetEmailService.SendEmailAsync(
                    bookingDto.Email,
                    Helpers.MailjetHelpers.MailjetTemplateType.BookingConfirmation,
                    new BookingConfirmationDto
                    {
                        Firstname = bookingDto.FirstName,
                        PickupAddress = bookingDto.PickUpAddress,
                        Stops = stopsText,
                        Destination = bookingDto.DropOffAddress,
                        PickupTime = formattedTime,
                        TotalPrice = booking.Price,
                    },
                    Helpers.MailjetHelpers.MailjetSubjects.BookingConfirmation
                    );
            }
        }


        private ServiceResponse<BookingResponseDto> BuildSuccessResponse(Bookings booking, bool isGuestBooking)
        {
            var response = _bookingMapper.MapToResponseDTO(booking);
            string message = isGuestBooking
                ? "Booking created successfully. Please check your email to confirm."
                : "Booking confirmed successfully!";

            return ServiceResponse<BookingResponseDto>.SuccessResponse(
                HttpStatusCode.OK,
                response,
                message
            );
        }

        #endregion
    }
}