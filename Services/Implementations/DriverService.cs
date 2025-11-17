using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Models.Roles;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Implementations.Base;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class DriverService : BaseBookingService, IDriverService
    {
        private readonly UserManager<Models.User> _userManager;
        private readonly IBookingValidationService _validationService;
        private readonly IMailjetEmailService _mailjetEmailService;
        private readonly ICarService _carService;

        public DriverService(
            UserManager<Models.User> userManager,
            IDriverRepo driverRepo,
            ILogger<DriverService> logger,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IBookingRepo bookingRepo,
            IBookingMapperService bookingMapper,
            IOptions<PaginationSettings> paginationSettings,
            IBookingValidationService validationService,
            IMapService mapService,
            IOptions<BookingRulesSettings> bookingRules,
            IMailjetEmailService mailjetEmailService,
            ICarService carService
        ) : base(bookingRepo, bookingMapper, userService, httpContextAccessor, paginationSettings, mapService, bookingRules, logger, driverRepo)
        {
            _userManager = userManager;
            _validationService = validationService;
            _mailjetEmailService = mailjetEmailService;
            _carService = carService;
        }

        public async Task<ServiceResponse<bool>> CreateDriverAsync(CreateRequestDriverDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProfilePicture))
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Profile picture is required"
                    );

                if (string.IsNullOrWhiteSpace(request.Email))
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Email is required"
                    );

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        "No user with that Id"
                    );

                if (user.IsDeleted)
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Cannot create driver for deleted user"
                    );

                var driverId = await _driverRepo.CreateDriver(request, user.Id);
               
                if (driverId == null)
                {
                    _logger.LogError("Driver repo failed");
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Failed to create driver");
                }

                var car = await _carService.CreateOrFindCarWithDriver(request.LicensePlate, driverId.Value);
                if (car == null )
                {
                    _logger.LogError("Failed to create or find car");
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to create or find car"
                    );
                }
                await _userManager.RemoveFromRoleAsync(user, UserRoles.User.ToString());
                await _userManager.AddToRoleAsync(user, UserRoles.Driver.ToString());

                return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.InternalServerError, "Failed");
            }
        }
        public async Task<ServiceResponse<UpdateDriverResponseDto>> UpdateDriverAsync(Guid driverId, UpdateRequestDriverDto updatedDriver, HttpContext httpContext)
        {
            try
            {
                if (!await CanUpdateDriver(httpContext, driverId))
                {
                    return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                        HttpStatusCode.Forbidden,
                        "You don't have permission to update this driver"
                    );
                }

                var result = await _driverRepo.UpdateDriver(updatedDriver, driverId);
                if (!result)
                {
                    return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to update driver"
                    );
                }

                var driver = await _driverRepo.GetDriverByIdAsync(driverId);
                var response = new UpdateDriverResponseDto
                {
                    DriverId = driverId,
                    ProfilePicture = driver?.ProfilePicture ?? string.Empty,
                    CarId = driver?.CarId
                };

                return ServiceResponse<UpdateDriverResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    response,
                    "Driver updated"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver {DriverId}", driverId);
                return ServiceResponse<UpdateDriverResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to update driver"
                );
            }
        }

        public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetDriverBookingsAsync(BookingSearchRequestDto query)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                        error.StatusCode, error.Message
                    );

                var bookingsQuery = BuildDriverBookingsQuery(driver.DriverId, query);
                return await GetPagedBookingsResponseAsync(bookingsQuery, query, "Found {count} booking(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDriverBookingsAsync failed");
                return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.InternalServerError, "Internal error");
            }
        }

        public async Task<ServiceResponse<BookingResponseDto>> AcceptBookingAsync(int bookingId)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(error.StatusCode, error.Message);

                var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
                if (booking is null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.NotFound, "Booking not found.");

                if (booking.PickUpDateTime <= DateTime.UtcNow)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "Pickup time is in the past.");

                if (!booking.IsAvailable || booking.DriverIdFK != null ||
                    booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.Conflict, "Booking is not available anymore.");

                var driverEntity = await _driverRepo.GetDriverEntityByIdAsync(driver!.DriverId);
                if (driverEntity == null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.NotFound, "Driver entity not found");

                var canDrive = await DriverAvailabilityAsync(driverEntity, booking);
                if (!canDrive)
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.BadRequest, "You don't have time between your other bookings.");

                booking.DriverIdFK = driver.DriverId;
                booking.IsAvailable = false;

                try
                {
                    var saved = await _bookingRepo.UpdateBookingAsync(booking);
                    if (!saved)
                        return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.Conflict, "Failed to accept booking.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.Conflict, "Booking was taken by someone else.");
                }

                var mappedBooking = _bookingMapper.MapToResponseDTO(booking);
                _logger.LogInformation("AcceptBookingAsync: Driver {DriverId} accepted booking {BookingId}.", driver.DriverId, bookingId);

                return ServiceResponse<BookingResponseDto>.SuccessResponse(HttpStatusCode.OK, mappedBooking, "Booking accepted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AcceptBookingAsync error");
                return ServiceResponse<BookingResponseDto>.FailResponse(HttpStatusCode.InternalServerError, "Unexpected error while accepting the booking.");
            }
        }

        public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<bool>.FailResponse(error.StatusCode, error.Message);

                var booking = await _bookingRepo.GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.DriverIdFK == driver!.DriverId && b.BookingId == bookingId);

                if (booking is null)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Booking not found or not assigned to you.");

                if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Booking is already finalised.");

                var validation = await _validationService.ValidatePickupTimeAsync(
                    booking.PickUpDateTime, _bookingRules.MinHoursBeforePickupForChange);

                if (validation.StatusCode != HttpStatusCode.OK)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Too late to unassign. Please contact an admin to reassign.");

                booking.DriverIdFK = null;
                booking.IsAvailable = true;

                try
                {
                    var saved = await _bookingRepo.UpdateBookingAsync(booking);
                    if (!saved)
                        return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Failed to release booking.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "State changed; release aborted.");
                }

                return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Booking released back to the bookingpool.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Driver unassign error for booking {BookingId}", bookingId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.InternalServerError, "Unexpected error while releasing the booking.");
            }
        }

        public async Task<ServiceResponse<bool>> ReassignBookingAsync(int bookingId, Guid newDriverId)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<bool>.FailResponse(error.StatusCode, error.Message);

                var booking = await _bookingRepo.GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.DriverIdFK == driver!.DriverId && b.BookingId == bookingId);

                if (booking == null)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Booking not found or not assigned to you.");

                if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Booking cannot be reassigned (completed/cancelled).");

                if (newDriverId == driver.DriverId)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Cannot reassign to yourself.");

                if (booking.PickUpDateTime <= DateTime.UtcNow)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Pickup time is in the past.");

                var newDriverEntity = await _driverRepo.GetDriverEntityByIdAsync(newDriverId);
                if (newDriverEntity == null || newDriverEntity.IsDeleted || (newDriverEntity.User != null && newDriverEntity.User.IsDeleted))
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Target driver is invalid.");

                var available = await CheckDriverAvailabilityForReassignAsync(newDriverId, booking);
                if (!available)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "The selected driver is not available for this booking.");

                booking.DriverIdFK = newDriverId;
                booking.IsAvailable = false;
                booking.Status = BookingStatus.Confirmed;

                try
                {
                    var saved = await _bookingRepo.UpdateBookingAsync(booking);
                    if (!saved)
                        return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Failed to reassign driver.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Booking was changed by someone else.");
                }

                _logger.LogInformation("Booking {BookingId} reassigned from driver {OldDriverId} to driver {NewDriverId}.",
                    bookingId, driver.DriverId, newDriverId);

                return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Booking reassigned successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning booking {BookingId}", bookingId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.InternalServerError, "Unexpected error while reassigning booking.");
            }
        }

        public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAvailableBookingsAsync(BookingSearchRequestDto query)
        {
            try
            {
                var bookingsQuery = _bookingRepo.GetAllQueryable(true)
                    .Where(b => b.IsAvailable && b.PickUpDateTime > DateTime.UtcNow);

                bookingsQuery = ApplyCommonFilters(bookingsQuery, query);

                return await GetPagedBookingsResponseAsync(bookingsQuery, query, "Listing {count} available booking(s).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAvailableBookingsAsync failed");
                return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.InternalServerError, "Internal error");
            }
        }


        public async Task<ServiceResponse<BookingResponseDto>> CompleteBookingAsync(int bookingId)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        error.StatusCode, error.Message
                    );

                var booking = await _bookingRepo.GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.DriverIdFK == driver.DriverId && b.BookingId == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("CompleteBookingAsync: Booking {BookingId} not found for driver {DriverId}", bookingId, driver.DriverId);
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found or not assigned to your account.");
                }

                if (booking.Status == BookingStatus.Completed)
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Booking is already marked as completed.");

                if (booking.Status == BookingStatus.Cancelled)
                    return ServiceResponse<BookingResponseDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "You cannot complete a cancelled booking.");

                booking.Status = BookingStatus.Completed;

                await _bookingRepo.UpdateBookingAsync(booking);

                var mappedBooking = _bookingMapper.MapToResponseDTO(booking);

                _logger.LogInformation(
                    "CompleteBookingAsync: Booking {BookingId} completed by driver {DriverId}.",
                    bookingId, driver.DriverId
                );

                // TODO: (Later) Send email to customer confirming completion

                return ServiceResponse<BookingResponseDto>.SuccessResponse(
                    HttpStatusCode.OK,
                    mappedBooking,
                    "Booking marked as completed successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CompleteBookingAsync for booking {BookingId}", bookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while completing the booking."
                );
            }
        }

        public async Task<ServiceResponse<DriverResponseDto>> GetCurrentDriverAsync()
        {
            var (driver, error) = await TryGetCurrentDriver();

            if (error != null)
            {
                return ServiceResponse<DriverResponseDto>.FailResponse(
                    error.StatusCode,
                    error.Message
                );
            }

            return ServiceResponse<DriverResponseDto>.SuccessResponse(
                HttpStatusCode.OK,
                driver!,
                "Driver profile retrieved successfully."
            );
        }


        public async Task<ServiceResponse<bool>> SendReceiptAsync(ReceiptRequestDto receiptDto)
        {
            try
            {
                var (driver, error) = await TryGetCurrentDriver();
                if (error != null)
                    return ServiceResponse<bool>.FailResponse(
                        error.StatusCode, error.Message
                    );

                var booking = await _bookingRepo.GetAllQueryable(true)
                    .FirstOrDefaultAsync(b => b.DriverIdFK == driver.DriverId && b.BookingId == receiptDto.BookingId);

                if (booking == null)
                {
                    _logger.LogWarning("SendReceiptAsync: Booking {BookingId} not found for driver {DriverId}",
                        receiptDto.BookingId, driver.DriverId);

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.NotFound,
                        "Booking not found or not assigned to this driver."
                    );
                }

                if (booking.Status != BookingStatus.Completed)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Receipt can only be sent for completed bookings."
                    );
                }

                var sendEmail = await _mailjetEmailService.SendReceiptEmailAsync(receiptDto);

                if (sendEmail.StatusCode != HttpStatusCode.OK || sendEmail.Data == false)
                {
                    _logger.LogWarning("SendReceiptAsync: Failed to send receipt email for booking {BookingId}. Status: {StatusCode}, Message: {Message}",
                        receiptDto.BookingId, sendEmail.StatusCode, sendEmail.Message);

                    return ServiceResponse<bool>.FailResponse(
                        sendEmail.StatusCode,
                        $"Failed to send receipt email: {sendEmail.Message}"
                    );
                }

                _logger.LogInformation("SendReceiptAsync: Successfully sent receipt for booking {BookingId} to customer.",
                    receiptDto.BookingId);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Receipt sent successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendReceiptAsync: Unexpected error while sending receipt for booking {BookingId}.",
                    receiptDto.BookingId);

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while sending the receipt."
                );
            }
        }

        #region Private Helpers
        private async Task<bool> CanUpdateDriver(HttpContext httpContext, Guid driverId)
        {
            var currentUser = await _userManager.GetUserAsync(httpContext.User);
            if (currentUser == null) return false;

            if (await _userManager.IsInRoleAsync(currentUser, UserRoles.Admin.ToString()))
                return true;
            var currentDriver = await _driverRepo.GetDriverByUserIdAsync(currentUser.Id);
            return currentDriver?.DriverId == driverId;
        }
        private async Task<(DriverResponseDto? driver, ServiceResponse<object>? error)> TryGetCurrentDriver()
        {
            var currentUser = await GetAuthenticatedUserAsync();
            if (currentUser == null)
                return (null, ServiceResponse<object>.FailResponse(HttpStatusCode.Unauthorized, "Please log in first!"));

            var driver = await _driverRepo.GetDriverByUserIdAsync(currentUser.Id);
            if (driver == null)
                return (null, ServiceResponse<object>.FailResponse(HttpStatusCode.Unauthorized, "No driver profile found for this user."));

            return (driver, null);
        }
        #endregion
    }
}
