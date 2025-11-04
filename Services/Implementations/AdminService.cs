using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

public class AdminService : IAdminService
{
    #region Dependencys
    private readonly IAdminRepo _adminRepo;
    private readonly ILogger<AdminService> _logger;
    private readonly PaginationSettings _paginationSettings;
    private readonly IBookingRepo _bookingRepo;
    private readonly IBookingMapperService _bookingMapper;
    private readonly IDriverRepo _driverRepo;
    private readonly IMapService _mapService;
    private readonly BookingRulesSettings _bookingRules;
    private readonly RecalculateIfAddressChangedHelper _recalculateHelper;
    private readonly ValidateUpdateRuleHelper _validateUpdateRuleHelper;
    private readonly IBookingService _bookingService;
    private readonly IBookingValidationService _validationService;

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
        IBookingValidationService validationService)
    {
        _adminRepo = adminRepo;
        _logger = logger;
        _paginationSettings = paginationOptions.Value;
        _bookingRepo = bookingRepo;
        _bookingMapper = bookingMapperService;
        _driverRepo = driverRepo;
        _mapService = mapService;
        _bookingRules = bookingRules.Value;
        _recalculateHelper = recalculateHelper;
        _validateUpdateRuleHelper = validateUpdateRuleHelper;
        _bookingService = bookingService;
        _validationService = validationService;
    }
    #endregion

    // TODOLATER: dont forget email sending functions. in assign driver
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

    public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAllBookingsAsync(BookingFilterRequestForAdminDto filterRequest)
    {
        try
        {
            var (currentPage, pageSize, sortBy, sortOrder) = ResolvePaginationSettings(filterRequest);
            var bookingsQuery = BuildAdminBookingsQuery(filterRequest);

            var pageResult = await bookingsQuery.ToPagedResultAsync(currentPage, pageSize, sortBy, sortOrder);
            var mappedResult = MapPagedResult(pageResult);

            _logger.LogInformation("GetAllBookingsAsync: Returned {Count} bookings.", mappedResult.Items.Count);

            return ServiceResponse<PaginatedResult<BookingResponseDto>>.SuccessResponse(
                HttpStatusCode.OK,
                mappedResult,
                $"There are {mappedResult.Items.Count} bookings listed successfully."
            );
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
            {
                _logger.LogWarning("AssignDriverAsync: Booking with ID {BookingId} not found.", bookingId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Could not find the booking!");
            }

            var driver = await _driverRepo.GetDriverEntityByIdAsync(driverId);
            if (driver is null)
            {
                _logger.LogWarning("AssignDriverAsync: Driver with ID {DriverId} not found.", driverId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.NotFound, "Could not find the driver!");
            }

            var isAvailable = await DriverAvailabilityAsync(driver, booking);
            if (!isAvailable)
            {
                _logger.LogInformation("AssignDriverAsync: Driver {DriverId} is not available for booking {BookingId}.", driverId, bookingId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Driver is not available for this booking.");
            }

            booking.DriverIdFK = driverId;
            booking.IsAvailable = false;
            await _bookingRepo.UpdateBookingAsync(booking);

            _logger.LogInformation("AssignDriverAsync: Driver {DriverId} assigned to booking {BookingId}.", driverId, bookingId);

            // --------------------TODO: Send email to customer and driver -----------------------------------///

            return ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK,
                true,
                "Driver is assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AssignDriverAsync: Unexpected error while assigning driver {DriverId} to booking {BookingId}.", driverId, bookingId);
            return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while assigning the driver.");
        }
    }

    public async Task<ServiceResponse<List<AvailableDriverResponsDto>>> GetAvailbleDrivers(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
            if (booking is null)
            {
                _logger.LogWarning("GetAvailbleDrivers: Booking with ID {BookingId} not found.", bookingId);
                return ServiceResponse<List<AvailableDriverResponsDto>>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Could not find the booking!");
            }

            var drivers = await _driverRepo.GetAllDriversAsync();

            var preliminaryDrivers = drivers
                .Where(d => !d.Bookings.Any(b =>
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Completed &&
                    b.PickUpDateTime < booking.PickUpDateTime.AddMinutes((double)booking.DurationMinutes) &&
                    b.PickUpDateTime.AddMinutes((double)b.DurationMinutes) > booking.PickUpDateTime
                ))
                .ToList();

            var freeDrivers = new List<Drivers>();

            foreach (var driver in preliminaryDrivers)
            {
                var canReachInTime = await DriverAvailabilityAsync(driver, booking);
                if (canReachInTime)
                    freeDrivers.Add(driver);
            }

            var availbleDriversDto = freeDrivers.Select(drivers => new AvailableDriverResponsDto
            {
                DriverId = drivers.DriverId, // I need to send their Id so frontend can assign a driver base on Id
                ProfilePicture = drivers.ProfilePicture,
                FullName = $"{drivers.User?.FirstName ?? "Unknown"} {drivers.User?.LastName ?? ""}".Trim(),
                PhoneNumber = drivers.User.PhoneNumber ?? "No phoneNumber registered!",
                CarModel = drivers.Car.Model ?? "This driver dosent have a car assignd!",
                RegistrationNumber = drivers.Car.LicensePlate ?? "N/A"
            }).ToList();

            _logger.LogInformation("GetAvailbleDrivers: Found {Count} available drivers for booking {BookingId}.", freeDrivers.Count, bookingId);

            return ServiceResponse<List<AvailableDriverResponsDto>>.SuccessResponse(
                HttpStatusCode.OK,
                availbleDriversDto,
                "Listing all available drivers for this booking.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAvailbleDrivers: Unexpected error while retrieving available drivers for booking {BookingId}.", bookingId);
            return ServiceResponse<List<AvailableDriverResponsDto>>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while retrieving available drivers.");
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

    #region Private Helpers
    private async Task<bool> DriverAvailabilityAsync(Drivers driver, Bookings newBooking)
    {

        var confirmedBookings = driver.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.PickUpDateTime)
            .ToList();

        var bookingBefore = confirmedBookings
            .LastOrDefault(b => b.PickUpDateTime < newBooking.PickUpDateTime);

        var bookingAfter = confirmedBookings
            .FirstOrDefault(b => b.PickUpDateTime > newBooking.PickUpDateTime);

        if (bookingBefore != null)
        {
            bool canTravelFromLast = await CanDriverTravelBetweenBookings(bookingBefore, newBooking);
            if (!canTravelFromLast)
            {
                return false;
            }
        }

        if (bookingAfter != null)
        {
            bool canTravelToNext = await CanDriverTravelBetweenBookings(newBooking, bookingAfter);
            if (!canTravelToNext)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> CanDriverTravelBetweenBookings(Bookings firstBooking, Bookings secondBooking)
    {
        var coordinates = new List<CoordinateDto>
        {
            new CoordinateDto
            {
                Latitude = (decimal)firstBooking.DropOffLatitude,
                Longitude = (decimal)firstBooking.DropOffLongitude
            },
            new CoordinateDto
            {
                Latitude = (decimal)secondBooking.PickUpLatitude,
                Longitude = (decimal)secondBooking.PickUpLongitude
            }
        };

        var routeResult = await _mapService.GetRouteInfoAsync(coordinates);
        if (routeResult.Data is null)
        {
            _logger.LogWarning("CanDriverTravelBetweenBookings: MapService API call failed.");
            return false;
        }

        var travelTimeNeeded = (double)routeResult.Data.DurationMinutes;
        var firstBookingEndTime = firstBooking.PickUpDateTime.AddMinutes((double)firstBooking.DurationMinutes);
        var secondBookingStartTime = secondBooking.PickUpDateTime;

        var availableTime = (secondBookingStartTime - firstBookingEndTime).TotalMinutes;

        var minutesBeforePickupBuffer = _bookingRules.MinMinutesBeforePickup;

        return availableTime >= (travelTimeNeeded + minutesBeforePickupBuffer);
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

    private IQueryable<Bookings> BuildAdminBookingsQuery(BookingFilterRequestForAdminDto filters)
    {
        var bookings = _bookingRepo.GetAllQueryable(true);

        bookings = bookings.ApplyFilters(filters);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.ToLower();

            bookings = bookings.Where(b =>
                b.PickUpAdress.ToLower().Contains(term) ||
                b.DropOffAdress.ToLower().Contains(term) ||
                (b.Flightnumber != null && b.Flightnumber.ToLower().Contains(term)) ||
                (b.Comment != null && b.Comment.ToLower().Contains(term)) ||

                ((b.GuestFirstName + " " + b.GuestLastName).ToLower().Contains(term)) ||
                (b.User != null && ((b.User.FirstName + " " + b.User.LastName).ToLower().Contains(term))) ||

                (b.GuestEmail != null && b.GuestEmail.ToLower().Contains(term)) ||
                (b.GuestPhoneNumber != null && b.GuestPhoneNumber.ToLower().Contains(term)) ||

                (b.Driver != null && (b.Driver.User.FirstName + " " + b.Driver.User.LastName).ToLower().Contains(term)) ||

                b.BookingId.ToString().Contains(term)
            );
        }

        return bookings;
    }

    private (int Page, int PageSize, string SortBy, string SortOrder) ResolvePaginationSettings(BookingFilterRequestForAdminDto filterRequest)
    {
        var settings = _paginationSettings;
        int page = filterRequest.Page ?? settings.DefaultPage;
        int pageSize = Math.Min(filterRequest.PageSize ?? settings.DefaultPageSize, settings.MaxPageSize);

        string sortBy = filterRequest.SortBy ?? settings.SortBy;
        string sortOrder = filterRequest.SortOrder.ToString().ToLower();

        return (page, pageSize, sortBy, sortOrder);
    }

    #endregion
}
