using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Implementations.Base;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

public class AdminService : BaseBookingService, IAdminService
{
    #region Dependencies
    private readonly IAdminRepo _adminRepo;
    private readonly RecalculateIfAddressChangedHelper _recalculateHelper;
    private readonly ValidateUpdateRuleHelper _validateUpdateRuleHelper;
    private readonly IBookingService _bookingService;
    private readonly IBookingValidationService _validationService;
    private readonly UserManager<User> _userManager;

    public AdminService(
        IAdminRepo adminRepo,
        ILogger<AdminService> logger,
        IOptions<PaginationSettings> paginationOptions,
        IBookingRepo bookingRepo,
        IBookingMapperService bookingMapperService,
        IDriverRepo driverRepo,
        IMapService mapService,
        IOptions<BookingRulesSettings> bookingRules,
        RecalculateIfAddressChangedHelper recalculateHelper,
        ValidateUpdateRuleHelper validateUpdateRuleHelper,
        IBookingService bookingService,
        IBookingValidationService validationService,
        IUserService userService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager)
        : base(bookingRepo, bookingMapperService, userService, httpContextAccessor, paginationOptions, mapService, bookingRules, logger, driverRepo)
    {
        _adminRepo = adminRepo;
        _recalculateHelper = recalculateHelper;
        _validateUpdateRuleHelper = validateUpdateRuleHelper;
        _bookingService = bookingService;
        _validationService = validationService;
        _userManager = userManager;
    }
    #endregion

    // TODOLATER: dont forget email sending functions. in assign driver
    #region Taxi settings
    public async Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync()
    {
        try
        {
            var taxiSettings = await _adminRepo.GetTaxiPricesAsync();

            if (taxiSettings is null)
            {
                _logger.LogWarning("GetTaxiPricesAsync: No price list found in database.");
                return ServiceResponse<TaxiSettings>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Couldnt find any price list in database!"
                );
            }

            _logger.LogInformation("GetTaxiPricesAsync: Retrieved latest taxi price settings.");
            return ServiceResponse<TaxiSettings>.SuccessResponse(
                HttpStatusCode.OK,
                taxiSettings,
                "Retrieving the latest price settings!"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTaxiPricesAsync: Unexpected error while retrieving taxi prices.");
            return ServiceResponse<TaxiSettings>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while retrieving taxi prices.");
        }
    }
    public async Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO)
    {
        try
        {
            var newSettings = new TaxiSettings
            {
                ZonePrice = taxiSettingsDTO.ZonePrice,
                KmPrice = taxiSettingsDTO.KmPrice,
                StartPrice = taxiSettingsDTO.StartPrice,
                MinutePrice = taxiSettingsDTO.MinutePrice,
                UpdatedAt = DateTime.UtcNow
            };

            await _adminRepo.CreateTaxiPricesAsync(newSettings);
            _logger.LogInformation("CreatePricesAsync: Created new taxi pricing settings.");

            return ServiceResponse<TaxiSettings>.SuccessResponse(
                HttpStatusCode.OK,
                newSettings,
                "Price settings have been updated!"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePricesAsync: Error while creating new taxi prices.");
            return ServiceResponse<TaxiSettings>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while creating taxi prices.");
        }
    }
    #endregion

    #region Booking Section
    public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAllBookingsAsync(BookingFilterRequestForAdminDto filterRequest)
    {
        try
        {
            var bookingsQuery = BuildAdminBookingsQuery(filterRequest);
            return await GetPagedBookingsResponseAsync(bookingsQuery, filterRequest, "There are {count} bookings listed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllBookingsAsync: Unexpected error while retrieving bookings.");
            return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while retrieving bookings.");
        }
    }

    public async Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

            if (booking is null)
            {
                _logger.LogWarning("GetBookingByIdAsync: Booking with ID {BookingId} not found.", bookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Could not get the booking from database!");
            }

            var mappedBooking = _bookingMapper.MapToResponseDTO(booking);
            _logger.LogInformation("GetBookingByIdAsync: Booking with ID {BookingId} retrieved.", bookingId);

            return ServiceResponse<BookingResponseDto>.SuccessResponse(
                HttpStatusCode.OK,
                mappedBooking,
                "Here is the booking!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBookingByIdAsync: Unexpected error while retrieving booking with ID {BookingId}.", bookingId);
            return ServiceResponse<BookingResponseDto>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while retrieving the booking.");
        }
    }

    public async Task<ServiceResponse<bool>> AssignDriverAsync(int bookingId, Guid driverId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
            if (booking is null)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Could not find the booking!");

            var driver = await _driverRepo.GetDriverEntityByIdAsync(driverId);
            if (driver is null)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Could not find the driver!");

            if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Booking cannot be assigned (completed/cancelled).");

            if (booking.PickUpDateTime <= DateTime.UtcNow)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Pickup time is in the past.");

            if (driver.IsDeleted || (driver.User != null && driver.User.IsDeleted))
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Driver is inactive/deleted.");

            var canReachInTime = await DriverAvailabilityAsync(driver, booking);
            if (!canReachInTime)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Driver is not available for this booking.");

            if (!booking.IsAvailable || booking.DriverIdFK != null)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Booking is not available anymore.");

            booking.DriverIdFK = driverId;
            booking.IsAvailable = false;
            booking.Status = BookingStatus.Confirmed;

            try
            {
                var saved = await _bookingRepo.UpdateBookingAsync(booking);
                if (!saved)
                    return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Failed to assign driver.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.Conflict, "Someone else assigned this booking first.");
            }

            // --------------------TODO: Send email to customer and driver -----------------------------------///

            _logger.LogInformation("AssignDriverAsync: Driver {DriverId} assigned to booking {BookingId}.", driverId, bookingId);
            return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Driver is assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AssignDriverAsync: Unexpected error");
            return ServiceResponse<bool>.FailResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred while assigning the driver.");
        }
    }

    public async Task<ServiceResponse<List<AvailableDriverResponsDto>>> GetAvailbleDrivers(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
            if (booking is null)
                return ServiceResponse<List<AvailableDriverResponsDto>>.FailResponse(HttpStatusCode.NotFound, "Could not find the booking!");

            var drivers = await _driverRepo.GetAllDriversAsync();

            var preliminaryDrivers = drivers
                .Where(d => !d.Bookings.Any(b =>
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed &&
                    b.PickUpDateTime < booking.PickUpDateTime.AddMinutes((double)booking.DurationMinutes) &&
                    b.PickUpDateTime.AddMinutes((double)b.DurationMinutes) > booking.PickUpDateTime))
                .ToList();

            var freeDrivers = new List<Drivers>();
            foreach (var driver in preliminaryDrivers)
            {
                var canReachInTime = await DriverAvailabilityAsync(driver, booking);
                if (canReachInTime) freeDrivers.Add(driver);
            }

            var availableDriversDto = freeDrivers.Select(d => new AvailableDriverResponsDto
            {
                DriverId = d.DriverId,
                ProfilePicture = d.ProfilePicture,
                FullName = $"{d.User?.FirstName ?? "Unknown"} {d.User?.LastName ?? ""}".Trim(),
                PhoneNumber = d.User?.PhoneNumber ?? "No phoneNumber registered!",
                CarModel = d.Car?.Model ?? "This driver doesn't have a car assigned!",
                RegistrationNumber = d.Car?.LicensePlate ?? "N/A"
            }).ToList();

            _logger.LogInformation("GetAvailbleDrivers: Found {Count} available drivers for booking {BookingId}.", availableDriversDto.Count, bookingId);

            return ServiceResponse<List<AvailableDriverResponsDto>>.SuccessResponse(HttpStatusCode.OK, availableDriversDto, "Listing all available drivers for this booking.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAvailbleDrivers: Unexpected error while retrieving available drivers for booking {BookingId}.", bookingId);
            return ServiceResponse<List<AvailableDriverResponsDto>>.FailResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred while retrieving available drivers.");
        }
    }

    public async Task<ServiceResponse<bool>> DeleteBookingByIdAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

            if (booking is null)
            {
                _logger.LogWarning("DeleteBookingByIdAsync: Booking with ID {BookingId} was not found.", bookingId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.NotFound,
                    $"Booking with ID {bookingId} was not found in the database.");
            }

            var result = await _bookingRepo.DeleteBookingAsync(booking);
            if (!result)
            {
                _logger.LogError("DeleteBookingByIdAsync: Failed to delete booking with ID {BookingId} from the database.", bookingId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "Failed to delete the booking from the database.");
            }

            _logger.LogInformation("DeleteBookingByIdAsync: Booking with ID {BookingId} deleted successfully.", bookingId);
            return ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK,
                true,
                "Booking deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteBookingByIdAsync: Unexpected error while deleting booking with ID {BookingId}.", bookingId);
            return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while deleting the booking.");
        }
    }

    public async Task<ServiceResponse<BookingResponseDto>> ChangeBookingById(UpdateBookingDto updateBookingDto)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(updateBookingDto.BookingId);

            if (booking is null)
            {
                _logger.LogWarning("GetBookingByIdAsync: Booking with ID {BookingId} not found.", updateBookingDto.BookingId);
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Could not get the booking from database!");
            }

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            {
                return ServiceResponse<BookingResponseDto>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Cannot update a booking that is already completed or cancelled.");
            }

            return await _bookingService.UpdateBookingInternalAsync(booking, updateBookingDto);

        }
        catch (Exception ex)
        {
            return ServiceResponse<BookingResponseDto>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while updating the booking.");
        }
    }

    public async Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);

            if (booking is null)
            {
                _logger.LogWarning("GetBookingByIdAsync: Booking with ID {BookingId} not found.", bookingId);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Could not get the booking from database!");
            }

            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Booking is already cancelled or completed.");

            booking.Status = BookingStatus.Cancelled;
            booking.IsAvailable = false;
            await _bookingRepo.UpdateBookingAsync(booking);

            return ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK, 
                true, 
                "Booking cancelled successfully.");
        }
        catch (Exception)
        {
            return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while cancelling the booking.");
        }
    }
    #endregion

    #region Driver Section
    public async Task<ServiceResponse<List<AllDriversRequestDto>>> GetAllDriversAsync()
    {
        try
        {
            var drivers = await _driverRepo.GetAllDriversAsync();

            var driverDtos = drivers.Select(d => new AllDriversRequestDto
            {
                Id = d.DriverId,
                FirstName = d.User.FirstName,
                LastName = d.User.LastName,
                ProfilePicture = d.ProfilePicture,
            }).ToList();

            string message = driverDtos.Count > 0
                ? $"Found {driverDtos.Count} driver(s)"
                : "No drivers found";

            return ServiceResponse<List<AllDriversRequestDto>>.SuccessResponse(
                HttpStatusCode.OK,
                driverDtos,
                message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service error in GetAllDriversAsync");
            return ServiceResponse<List<AllDriversRequestDto>>.FailResponse(
                HttpStatusCode.InternalServerError,
                "Failed to retrieve drivers"
            );
        }
    }
    public async Task<ServiceResponse<DriverResponseDto>> GetDriverByUserIdAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ServiceResponse<DriverResponseDto>.FailResponse(
                   HttpStatusCode.NotFound,
                   "Failed to find user"
               );
            }
            var driver = await _driverRepo.GetDriverByUserIdAsync(user.Id);

            if (driver == null)
            {
                return ServiceResponse<DriverResponseDto>.FailResponse(
                   HttpStatusCode.NotFound,
                   "Failed to find driver"
               );
            }
            var repsonse = new DriverResponseDto
            {
                DriverId = driver.DriverId,
                FirstName = driver.FirstName,
                LastName = driver.LastName,
                ProfilePicture = driver.ProfilePicture,
                CarId = driver.CarId
            };
            return ServiceResponse<DriverResponseDto>.SuccessResponse(
                   HttpStatusCode.OK,
                   repsonse,
                   "Found driver"
               );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geting driver");
            return ServiceResponse<DriverResponseDto>.FailResponse(
                HttpStatusCode.InternalServerError,
                "Error while to get driver"
            );
        }


    }
    public async Task<ServiceResponse<DriverResponseDto>> GetDriverByIdAsync(Guid driverId)
    {
        try
        {
            var driver = await _driverRepo.GetDriverByIdAsync(driverId);

            if (driver == null)
            {
                return ServiceResponse<DriverResponseDto>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Driver not found"
                );
            }

            return ServiceResponse<DriverResponseDto>.SuccessResponse(
                HttpStatusCode.OK,
                driver,
                "Driver retrieved"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving driver {DriverId}", driverId);
            return ServiceResponse<DriverResponseDto>.FailResponse(
                HttpStatusCode.InternalServerError,
                "Failed to retrieve driver"
            );
        }
    }
    public async Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId)
    {
        try
        {
            var driver = await _driverRepo.GetDriverByIdAsync(driverId);

            if (driver == null)
            {
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Driver not found"
                );
            }

            var result = await _driverRepo.DeleteDriver(driverId);

            if (!result)
            {
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.BadRequest,
                    "Failed to delete driver"
                );
            }

            return ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK,
                true,
                "Driver deleted"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting driver {DriverId}", driverId);
            return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.InternalServerError,
                "Failed to delete driver"
            );
        }
    }
    #endregion

    #region Private Helpers
    private IQueryable<Bookings> BuildAdminBookingsQuery(BookingFilterRequestForAdminDto filters)
    {
        var bookings = _bookingRepo.GetAllQueryable(true);
        bookings = ApplyCommonFilters(bookings, filters);
        bookings = bookings.ApplyFilters(filters);
        return bookings;
    }

    #endregion
}
