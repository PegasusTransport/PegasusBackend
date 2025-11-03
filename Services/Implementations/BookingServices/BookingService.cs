using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers;
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
        private readonly BookingRulesSettings _bookingRules;
        private readonly IWebHostEnvironment _env;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PaginationSettings _paginationSettings;
        private readonly RecalculateIfAddressChangedHelper _recalculateHelper;
        private readonly ValidateUpdateRuleHelper _validateUpdateRuleHelper;

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
            IHttpContextAccessor httpContextAccessor,
            IOptions<PaginationSettings> paginationSettings,
            RecalculateIfAddressChangedHelper recalculateHelper,
            ValidateUpdateRuleHelper validateUpdateRuleHelper)
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
            _paginationSettings = paginationSettings.Value;
            _recalculateHelper = recalculateHelper;
            _validateUpdateRuleHelper = validateUpdateRuleHelper;
        }
        #endregion

        /* TO DO
            – Send cancellation emails to driver, admin and customer
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

        public async Task<ServiceResponse<BookingResponseDto>> UpdateBookingForUserAsync(UpdateBookingDto updateDto)
        {
            try
            {
                var currentUser = await GetAuthenticatedUserAsync();
                if (currentUser == null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "You must be logged in to view your bookings."
                    );

                var booking = await _bookingRepo
                    .GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.BookingId == updateDto.BookingId && b.UserIdFk == currentUser.Id);

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

                return await UpdateBookingInternalAsync(booking, updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", updateDto.BookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Something went wrong while updating the booking.");
            }
        }

        // This method can be used in driverServcie and AdminService. It only contains the logic for updating a booking without any user!! 
        public async Task<ServiceResponse<BookingResponseDto>> UpdateBookingInternalAsync(Bookings booking, UpdateBookingDto updateDto)
        {
            var ruleValidation = await _validateUpdateRuleHelper.ValidateUpdateRulesAsync(booking, updateDto);
            if (ruleValidation != null)
                return ruleValidation;

            var recalcResponse = await _recalculateHelper.RecalculateIfAddressChangedAsync(booking, updateDto);
            if (recalcResponse != null)
                return recalcResponse;

            UpdateBookingFields(booking, updateDto);

            if (!await _bookingRepo.UpdateBookingAsync(booking))
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to update booking in the database."
                );

            var result = _bookingMapper.MapToResponseDTO(booking);
            return ServiceResponse<BookingResponseDto>.SuccessResponse(HttpStatusCode.OK, result, "Booking updated successfully.");
        }

        public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetMyBookingsAsync(BookingSearchRequestDto query)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
                return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.Unauthorized,
                    "You must be logged in to view your bookings."
                );

            var (currentPage, pageSize, sortBy, sortOrder) = ResolvePaginationSettings(query);
            var bookingsQuery = BuildUserBookingsQuery(user.Id, query);

            var pagedResult = await bookingsQuery.ToPagedResultAsync(currentPage, pageSize, sortBy, sortOrder);

            var mappedResult = MapPagedResult(pagedResult);

            return ServiceResponse<PaginatedResult<BookingResponseDto>>.SuccessResponse(
                HttpStatusCode.OK,
                mappedResult,
                $"You have {mappedResult.Items.Count} bookings listed successfully."
            );
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId)
        {
            try
            {
                var currentUser = await GetAuthenticatedUserAsync();
                if (currentUser == null)
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Unauthorized,
                        "You must be logged in to view your bookings."
                    );

                var booking = await _bookingRepo
                    .GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.UserIdFk == currentUser.Id);

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
        private async Task<UserResponseDto?> GetAuthenticatedUserAsync()
        {
            var result = await _userService.GetLoggedInUser(_httpContextAccessor.HttpContext);
            return result?.Data;
        }

        private (int Page, int PageSize, string SortBy, string SortOrder) ResolvePaginationSettings(BookingSearchRequestDto query)
        {
            var settings = _paginationSettings;
            int page = query.Page ?? settings.DefaultPage;
            int pageSize = Math.Min(query.PageSize ?? settings.DefaultPageSize, settings.MaxPageSize);
            string sortBy = query.SortBy ?? settings.SortBy;
            string sortOrder = query.SortOrder.ToString().ToLower();

            return (page, pageSize, sortBy, sortOrder);
        }

        private IQueryable<Bookings> BuildUserBookingsQuery(string userId, BookingSearchRequestDto query)
        {
            var bookings = _bookingRepo.GetAllQueryable(true)
                .Where(b => b.UserIdFk == userId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.ToLower();
                bookings = bookings.Where(b =>
                    b.PickUpAdress.ToLower().Contains(term) ||
                    b.DropOffAdress.ToLower().Contains(term) ||
                    (b.Flightnumber != null && b.Flightnumber.ToLower().Contains(term)) ||
                    (b.Comment != null && b.Comment.ToLower().Contains(term))
                );
            }

            if (query.MinPrice.HasValue)
                bookings = bookings.Where(b => b.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                bookings = bookings.Where(b => b.Price <= query.MaxPrice.Value);

            if (query.FromDate.HasValue)
                bookings = bookings.Where(b => b.PickUpDateTime >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                bookings = bookings.Where(b => b.PickUpDateTime <= query.ToDate.Value);

            if (query.Status.HasValue)
            {
                var statusValue = (BookingStatus)query.Status.Value;
                bookings = bookings.Where(b => b.Status == statusValue);
            }

            if (query.UpcomingOnly == true)
                bookings = bookings.Where(b => b.PickUpDateTime > DateTime.UtcNow);

            return bookings;
        }

        private PaginatedResult<BookingResponseDto> MapPagedResult(PaginatedResult<Bookings> paged)
        {
            return new PaginatedResult<BookingResponseDto>
            {
                Items = _bookingMapper.MapToResponseDTOs(paged.Items),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize,
                CurrentCount = paged.CurrentCount
            };
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
