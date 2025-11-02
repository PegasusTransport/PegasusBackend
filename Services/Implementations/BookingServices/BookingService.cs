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
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace PegasusBackend.Services.Implementations.BookingServices
{
    public class BookingService : IBookingService
    {
        #region Dependencies
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
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

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
            IWebHostEnvironment env,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
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
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        /* TO DO
            – Send cancellation emails to driver, admin and customer
            – Restrict address change within 24h of pickup
            - Add pagination and search for customer!
        */

        public async Task<ServiceResponse<BookingResponseDto>> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            try
            {
                var validation = await _validationService.ValidateBookingAsync(bookingDto);
                if (!validation.IsValid)
                    return validation.ErrorResponse!;

                var user = await _userManager.FindByEmailAsync(bookingDto.Email);
                var isGuest = user == null;

                var booking = _bookingFactory.CreateBookingEntity(
                    bookingDto,
                    validation.RouteInfo!,
                    validation.CalculatedPrice,
                    user,
                    isGuest);

                await _bookingRepo.CreateBookingAsync(booking);
                await SendBookingEmailAsync(booking, bookingDto, isGuest);

                return BuildSuccessResponse(booking, isGuest);
            }
            catch (Exception ex)
            {
                return HandleError<BookingResponseDto>(ex, "creating booking");
            }
        }

        public async Task<ServiceResponse<BookingPreviewResponseDto>> GetBookingPreviewAsync(BookingPreviewRequestDto previewDto)
        {
            try
            {
                var bookingDto = ConvertPreviewToBookingDto(previewDto);
                var validation = await _validationService.ValidateBookingAsync(bookingDto);

                if (!validation.IsValid)
                    return ServiceResponse<BookingPreviewResponseDto>.FailResponse(
                        validation.ErrorResponse!.StatusCode,
                        validation.ErrorResponse.Message);

                var response = new BookingPreviewResponseDto
                {
                    DistanceKm = validation.RouteInfo!.DistanceKm,
                    DurationMinutes = validation.RouteInfo.DurationMinutes,
                    Price = Math.Round(validation.CalculatedPrice, 2),
                    Sections = validation.RouteInfo.Sections,
                    Message = "Calculated price for your trip."
                };

                return ServiceResponse<BookingPreviewResponseDto>.SuccessResponse(
                    HttpStatusCode.OK, response, "Price preview calculated.");
            }
            catch (Exception ex)
            {
                return HandleError<BookingPreviewResponseDto>(ex, "getting booking preview");
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> ConfirmBookingAsync(string confirmationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(confirmationToken))
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Invalid confirmation token.");

                var booking = await _bookingRepo.GetBookingByConfirmationTokenAsync(confirmationToken);
                if (booking == null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.NotFound, "Booking not found.");

                if (booking.ConfirmationTokenExpiresAt < DateTime.UtcNow)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Confirmation token has expired.");

                if (booking.IsConfirmed)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Booking is already confirmed.");

                booking.Status = BookingStatus.Confirmed;
                booking.IsConfirmed = true;
                booking.IsAvailable = true;
                booking.ConfirmationToken = null;
                booking.ConfirmationTokenExpiresAt = null;

                await _bookingRepo.UpdateBookingAsync(booking);
                var response = _bookingMapper.MapToResponseDTO(booking);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(HttpStatusCode.OK, response, "Booking confirmed successfully.");
            }
            catch (Exception ex)
            {
                return HandleError<BookingResponseDto>(ex, "confirming booking");
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> UpdateBookingAsync(UpdateBookingDto updateDto)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var currentUser = await _userService.GetLoggedInUser(httpContext);

                if (currentUser.Data == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "You must be logged in to update a booking."
                    );
                }

                var booking = await _bookingRepo
                    .GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.BookingId == updateDto.BookingId && b.UserIdFk == currentUser.Data.Id);

                if (booking == null)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Couldnt find the booking in database. Check that you are Authorize!");
                }

                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Cannot update a booking that is already completed or cancelled.");
                }

                var ruleValidation = await ValidateUpdateRulesAsync(booking, updateDto);
                if (ruleValidation != null)
                    return ruleValidation;

                var recalculateResponse = await RecalculateIfAddressChangedAsync(booking, updateDto);
                if (recalculateResponse != null)
                    return recalculateResponse;

                UpdateBookingFields(booking, updateDto);

                if (!await _bookingRepo.UpdateBookingAsync(booking))
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        "Failed to update booking in the database.");

                var result = _bookingMapper.MapToResponseDTO(booking);
                return ServiceResponse<BookingResponseDto>.SuccessResponse(HttpStatusCode.OK, result, "Booking updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", updateDto.BookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while updating the booking.");
            }
        }

        public async Task<ServiceResponse<List<BookingResponseDto>>> GetMyBookingsAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var currentUser = await _userService.GetLoggedInUser(httpContext);

            if (currentUser.Data == null)
                return ServiceResponse<List<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.Unauthorized,
                    "You must be logged in to view your bookings."
                );

            var bookings = await _bookingRepo.GetUserBookingsAsync(currentUser.Data.Id);
            var bookingList = _bookingMapper.MapToResponseDTOs(bookings);

            return ServiceResponse<List<BookingResponseDto>>.SuccessResponse(
                HttpStatusCode.OK,
                bookingList,
                 $"You have {bookingList.Count} bookings listed successfully."
            );
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var currentUser = await _userService.GetLoggedInUser(httpContext);

                if (currentUser.Data == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "You must be logged in to cancel a booking."
                    );
                }

                var booking = await _bookingRepo
                    .GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserIdFk == currentUser.Data.Id);

                if (booking is null)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Couldnt fetch data from database! Check that you are Authorize!");

                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Booking is already cancelled or completed.");

                var validation = await _validationService.ValidatePickupTimeAsync(
                    booking.PickUpDateTime, _bookingRules.MinHoursBeforePickupForChange);

                if (validation.StatusCode != HttpStatusCode.OK)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.Forbidden, "Too late to cancel. Please contact support.");

                booking.Status = BookingStatus.Cancelled;
                booking.IsAvailable = false;
                await _bookingRepo.UpdateBookingAsync(booking);

                return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Booking cancelled successfully.");
            }
            catch (Exception ex)
            {
                return HandleError<bool>(ex, "cancelling booking");
            }
        }

        #region Private Helpers
        private async Task<ServiceResponse<BookingResponseDto>?> ValidateUpdateRulesAsync(Bookings booking, UpdateBookingDto dto)
        {
            // How about if they change adresses? maybe the driver dosent have time for their next customer?
            if (booking.PickUpDateTime != dto.PickUpDateTime)
            {
                var result = await _validationService.ValidatePickupTimeAsync(dto.PickUpDateTime, _bookingRules.MinHoursBeforePickupForChange);
                if (result.StatusCode != HttpStatusCode.OK)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.Forbidden, "It's too late to change your pickup time.");
            }

            return null;
        }

        private async Task<ServiceResponse<BookingResponseDto>?> RecalculateIfAddressChangedAsync(Bookings booking, UpdateBookingDto dto)
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

        private void UpdateBookingFields(Bookings booking, UpdateBookingDto dto)
        {
            booking.PickUpDateTime = dto.PickUpDateTime;
            booking.Flightnumber = dto.Flightnumber;
            booking.Comment = dto.Comment;
            booking.PickUpAdress = dto.PickUpAddress;
            booking.PickUpLatitude = dto.PickUpLatitude;
            booking.PickUpLongitude = dto.PickUpLongitude;
            booking.FirstStopAddress = dto.FirstStopAddress;
            booking.FirstStopLatitude = dto.FirstStopLatitude;
            booking.FirstStopLongitude = dto.FirstStopLongitude;
            booking.SecondStopAddress = dto.SecondStopAddress;
            booking.SecondStopLatitude = dto.SecondStopLatitude;
            booking.SecondStopLongitude = dto.SecondStopLongitude;
            booking.DropOffAdress = dto.DropOffAddress;
            booking.DropOffLatitude = dto.DropOffLatitude;
            booking.DropOffLongitude = dto.DropOffLongitude;
        }

        private async Task SendBookingEmailAsync(Bookings booking, CreateBookingDto dto, bool isGuest)
        {
            var stops = BookingMailHelper.FormatStops(dto);
            var time = BookingMailHelper.FormatDateTime(dto.PickUpDateTime);

            var template = isGuest
                ? Helpers.MailjetHelpers.MailjetTemplateType.PendingConfirmation
                : Helpers.MailjetHelpers.MailjetTemplateType.BookingConfirmation;

            var subject = isGuest
                ? Helpers.MailjetHelpers.MailjetSubjects.PendingConfirmation
                : Helpers.MailjetHelpers.MailjetSubjects.BookingConfirmation;

            var baseUrl = _env.IsDevelopment()
                ? _settings.Links.LocalConfirmationBase
                : _settings.Links.ProductionConfirmationBase;

            var confirmationLink = $"{baseUrl}{booking.ConfirmationToken}";

            if (isGuest)
            {
                await _mailjetEmailService.SendEmailAsync(dto.Email, template, new PendingConfirmationRequestDto
                {
                    Firstname = dto.FirstName,
                    PickupAddress = dto.PickUpAddress,
                    Stops = stops,
                    Destination = dto.DropOffAddress,
                    PickupTime = time,
                    TotalPrice = booking.Price,
                    ConfirmationLink = confirmationLink
                }, subject);
            }
            else
            {
                await _mailjetEmailService.SendEmailAsync(dto.Email, template, new BookingConfirmationRequestDto
                {
                    Firstname = dto.FirstName,
                    PickupAddress = dto.PickUpAddress,
                    Stops = stops,
                    Destination = dto.DropOffAddress,
                    PickupTime = time,
                    TotalPrice = booking.Price
                }, subject);
            }
        }

        private ServiceResponse<T> HandleError<T>(
            Exception ex,
            [CallerMemberName] string action = "",
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
            string? message = null)
        {
            _logger.LogError(ex, "Error in {Action}", action);
            var msg = message ?? $"Something went wrong while {action}.";
            return ServiceResponse<T>.FailResponse(statusCode, msg);
        }

        private ServiceResponse<BookingResponseDto> BuildSuccessResponse(Bookings booking, bool isGuest)
        {
            var response = _bookingMapper.MapToResponseDTO(booking);
            var message = isGuest
                ? "Booking created successfully. Please confirm via email."
                : "Booking confirmed successfully.";
            return ServiceResponse<BookingResponseDto>.SuccessResponse(HttpStatusCode.OK, response, message);
        }

        private static CreateBookingDto ConvertPreviewToBookingDto(BookingPreviewRequestDto dto) =>
            new()
            {
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PhoneNumber = string.Empty,
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
                DropOffLongitude = dto.DropOffLongitude,
                Flightnumber = dto.Flightnumber
            };

        #endregion
    }
}
