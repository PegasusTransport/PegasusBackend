using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.BookingHelpers;
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
        #region
        private readonly IBookingRepo _bookingRepo;
        private readonly UserManager<User> _userManager;
        private readonly IMailjetEmailService _mailjetEmailService;
        private readonly IBookingValidationService _validationService;
        private readonly IBookingFactoryService _bookingFactory;
        private readonly IBookingMapperService _bookingMapper;
        private readonly ILogger<BookingService> _logger;
        private readonly MailJetSettings _settings;
        private readonly IWebHostEnvironment _env;
        private readonly BookingRulesSettings _bookingRules;

        public BookingService(
            IBookingRepo bookingRepo,
            UserManager<User> userManager,
            IMailjetEmailService mailjetEmailService,
            IBookingValidationService validationService,
            IBookingFactoryService bookingFactory,
            IBookingMapperService bookingMapper,
            ILogger<BookingService> logger,
            IOptions<MailJetSettings> mailJetSettings,
            IOptions<BookingRulesSettings> bookingRules,
            IWebHostEnvironment env)
        {
            _bookingRepo = bookingRepo;
            _userManager = userManager;
            _mailjetEmailService = mailjetEmailService;
            _validationService = validationService;
            _bookingFactory = bookingFactory;
            _bookingMapper = bookingMapper;
            _logger = logger;
            _settings = mailJetSettings.Value;
            _bookingRules = bookingRules.Value;
            _env = env;
        }

        #endregion

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

        public async Task<ServiceResponse<BookingPreviewResponseDto>> GetBookingPreviewAsync(
            BookingPreviewRequestDto previewDto)
        {
            try
            {
                // Convert to CreateBookingDto to reuse existing validation
                var bookingDto = ConvertPreviewToBookingDto(previewDto);

                // Reuses the exact same validation as CreateBookingAsync
                var validationResult = await _validationService.ValidateBookingAsync(bookingDto);

                if (!validationResult.IsValid)
                {
                    return ServiceResponse<BookingPreviewResponseDto>.FailResponse(
                        validationResult.ErrorResponse!.StatusCode,
                        validationResult.ErrorResponse.Message
                    );
                }

                // Build response with calculated data
                var response = new BookingPreviewResponseDto
                {
                    DistanceKm = validationResult.RouteInfo!.DistanceKm,
                    DurationMinutes = validationResult.RouteInfo.DurationMinutes,
                    Price = Math.Round(validationResult.CalculatedPrice, 2),
                    Message = "Calculated price for your trip.",
                    Sections = validationResult.RouteInfo.Sections
                };

                return ServiceResponse<BookingPreviewResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Price preview calculated."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating booking preview");
                return ServiceResponse<BookingPreviewResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong during price calculation."
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
                    // I HAVE TO FIX THIS!!! IT CANNOT REMOVE THE BOOKING JUST BC THE TOKEN IS EXMIRED! Maybe send a new token!
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

        public async Task<ServiceResponse<BookingResponseDto>> UpdateBookingAsync(
            UpdateBookingDto updateDto,
            string? token = null,
            ClaimsPrincipal? user = null)
        {
            try
            {
                var booking = await GetBookingAsync(updateDto, token, user);
                if (booking == null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Could not find the booking in the database."
                    );
                
                var validation = await ValidateUpdateRulesAsync(booking, updateDto);
                if (validation != null)
                    return validation;

                await RecalculateIfAddressChangedAsync(booking, updateDto);

                UpdateBookingFields(booking, updateDto);

                var updateSuccess = await _bookingRepo.UpdateBookingAsync(booking);
                if (!updateSuccess)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        "Failed to update booking in the database."
                    );
                }

                _logger.LogInformation("Booking {BookingId} updated successfully.", booking.BookingId);
                var result = _bookingMapper.MapToResponseDTO(booking);
                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    result,
                    "Booking updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", updateDto.BookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while updating the booking."
                );
            }
        }



        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        $"Couldnt find booking with id {bookingId}");

                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "This booking is already cancled or completed!");

                var cancellationTime = await _validationService.ValidatePickupTimeAsync(booking.PickUpDateTime, _bookingRules.MinHoursBeforePickupForChange);
                if (cancellationTime.StatusCode != HttpStatusCode.OK)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "It's too late to cancel your booking. Please contact support for urgent changes."
                    );
                }

                booking.Status = BookingStatus.Cancelled;
                booking.IsAvailable = false;

                await _bookingRepo.UpdateBookingAsync(booking); // I dont delete the booking from database so it can be ther for statistics in future!

                // Here I can send a Email to customer and the driver that the booking is cancelled!
                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "The booking is succesfully and unfurtunally cancelled!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while cancelling the booking."
                );
            }
        }

        #region Private Helper Methods

        private async Task<Bookings?> GetBookingAsync(UpdateBookingDto updateDto, string? token, ClaimsPrincipal? user)
        {
            if (!string.IsNullOrEmpty(token))
                return await _bookingRepo.GetBookingByConfirmationTokenAsync(token);

            if (user != null)
            {
                var userId = _userManager.GetUserId(user);
                return await _bookingRepo.GetAllQueryable(false)
                    .FirstOrDefaultAsync(b => b.BookingId == updateDto.BookingId && b.UserIdFk == userId);
            }

            return null;
        }

        private void UpdateBookingFields(Bookings booking, UpdateBookingDto updateDto)
        {
            booking.PickUpDateTime = updateDto.PickUpDateTime;
            booking.Flightnumber = updateDto.Flightnumber;
            booking.Comment = updateDto.Comment;
            booking.FirstStopAddress = updateDto.FirstStopAddress;
            booking.FirstStopLatitude = updateDto.FirstStopLatitude;
            booking.FirstStopLongitude = updateDto.FirstStopLongitude;
            booking.SecondStopAddress = updateDto.SecondStopAddress;
            booking.SecondStopLatitude = updateDto.SecondStopLatitude;
            booking.SecondStopLongitude = updateDto.SecondStopLongitude;
            booking.DropOffAdress = updateDto.DropOffAddress;
            booking.DropOffLatitude = updateDto.DropOffLatitude;
            booking.DropOffLongitude = updateDto.DropOffLongitude;
        }

        private async Task RecalculateIfAddressChangedAsync(Bookings booking, UpdateBookingDto updateDto)
        {
            bool addressChanged =
                booking.PickUpAdress != updateDto.PickUpAddress ||
                booking.FirstStopAddress != updateDto.FirstStopAddress ||
                booking.SecondStopAddress != updateDto.SecondStopAddress ||
                booking.DropOffAdress != updateDto.DropOffAddress;

            if (!addressChanged)
                return;

            var tempDto = new CreateBookingDto
            {
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PhoneNumber = string.Empty,
                PickUpDateTime = updateDto.PickUpDateTime,
                PickUpAddress = updateDto.PickUpAddress,
                PickUpLatitude = updateDto.PickUpLatitude,
                PickUpLongitude = updateDto.PickUpLongitude,
                FirstStopAddress = updateDto.FirstStopAddress,
                FirstStopLatitude = updateDto.FirstStopLatitude,
                FirstStopLongitude = updateDto.FirstStopLongitude,
                SecondStopAddress = updateDto.SecondStopAddress,
                SecondStopLatitude = updateDto.SecondStopLatitude,
                SecondStopLongitude = updateDto.SecondStopLongitude,
                DropOffAddress = updateDto.DropOffAddress,
                DropOffLatitude = updateDto.DropOffLatitude,
                DropOffLongitude = updateDto.DropOffLongitude,
                Flightnumber = updateDto.Flightnumber,
                Comment = updateDto.Comment
            };

            var routeResult = await _validationService.VerifyRouteAsync(tempDto);
            if (!routeResult.IsValid)
                throw new InvalidOperationException("Invalid route.");

            var priceResult = await _validationService.CalculateAndVerifyPriceAsync(tempDto, routeResult.RouteInfo!);
            if (!priceResult.IsValid)
                throw new InvalidOperationException("Price calculation failed.");

            booking.DistanceKm = routeResult.RouteInfo!.DistanceKm;
            booking.DurationMinutes = routeResult.RouteInfo.DurationMinutes;
            booking.Price = priceResult.Price;
        }

        private async Task<ServiceResponse<BookingResponseDto>?> ValidateUpdateRulesAsync(Bookings booking, UpdateBookingDto updateDto)
        {
            var pickupValidation = await _validationService.ValidatePickupTimeAsync(
                updateDto.PickUpDateTime,
                _bookingRules.MinHoursBeforePickupForChange
            );

            if (pickupValidation.StatusCode != HttpStatusCode.OK)
            {
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.Forbidden,
                    "It's too late to change your pickup time. Please contact support for urgent changes."
                );
            }

            return null;
        }

        private async Task SendBookingEmailAsync(Bookings booking, CreateBookingDto bookingDto, bool isGuestBooking)
        {
            var stopsText = BookingMailHelper.FormatStops(bookingDto);
            var formattedTime = BookingMailHelper.FormatDateTime(bookingDto.PickUpDateTime);


            var baseUrl = _env.IsDevelopment()
            ? _settings.Links.LocalConfirmationBase
            : _settings.Links.ProductionConfirmationBase;

            var confirmationLink = $"{baseUrl}{booking.ConfirmationToken}";

            if (isGuestBooking)
            {

                await _mailjetEmailService.SendEmailAsync(
                    bookingDto.Email,
                    Helpers.MailjetHelpers.MailjetTemplateType.PendingConfirmation,
                    new PendingConfirmationRequestDto
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
                    new BookingConfirmationRequestDto
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

        private static CreateBookingDto ConvertPreviewToBookingDto(BookingPreviewRequestDto previewDto)
        {
            return new CreateBookingDto
            {
                // Dummy customer-data (required for validation but never used)
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PhoneNumber = string.Empty,

                // Actual route data from the user
                PickUpDateTime = previewDto.PickUpDateTime,
                PickUpAddress = previewDto.PickUpAddress,
                PickUpLatitude = previewDto.PickUpLatitude,
                PickUpLongitude = previewDto.PickUpLongitude,
                FirstStopAddress = previewDto.FirstStopAddress,
                FirstStopLatitude = previewDto.FirstStopLatitude,
                FirstStopLongitude = previewDto.FirstStopLongitude,
                SecondStopAddress = previewDto.SecondStopAddress,
                SecondStopLatitude = previewDto.SecondStopLatitude,
                SecondStopLongitude = previewDto.SecondStopLongitude,
                DropOffAddress = previewDto.DropOffAddress,
                DropOffLatitude = previewDto.DropOffLatitude,
                DropOffLongitude = previewDto.DropOffLongitude,
                Flightnumber = previewDto.Flightnumber
            };
        }

        #endregion
    }
}