using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

public class AdminService : IAdminService
{
    private readonly IAdminRepo _adminRepo;
    private readonly ILogger<AdminService> _logger;
    private readonly PaginationSettings _paginationSettings;
    private readonly IBookingRepo _bookingRepo;
    private readonly IBookingMapperService _bookingMapper;
    private readonly IDriverRepo _driverRepo;
    private readonly IMapService _mapService;
    private readonly BookingRulesSettings _bookingRules;

    public AdminService(
        IAdminRepo adminRepo,
        ILogger<AdminService> logger,
        IOptions<PaginationSettings> paginationOptions,
        IBookingRepo bookingRepo,
        IBookingMapperService bookingMapperService,
        IDriverRepo driverRepo,
        IMapService mapService,
        IOptions<BookingRulesSettings> bookingRules)
    {
        _adminRepo = adminRepo;
        _logger = logger;
        _paginationSettings = paginationOptions.Value;
        _bookingRepo = bookingRepo;
        _bookingMapper = bookingMapperService;
        _driverRepo = driverRepo;
        _mapService = mapService;
        _bookingRules = bookingRules.Value;
    }

    // Remember to add controller for admin to cancell a booking!!!!!
    // dont forget email sending functions. in assign driver
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

    public async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAllBookingsAsync(
        BookingFilterDto? filters, BookingSearchRequestDto searchRequestDto)
    {
        try
        {
            var (currentPage, pageSize, sortBy, sortOrder) = ResolvePaginationSettings(searchRequestDto);
            var bookingsQuery = BuildAdminBookingsQuery(searchRequestDto);

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

            var isAvailable = await DriverAvibility(driver, booking);
            if (!isAvailable)
            {
                _logger.LogInformation("AssignDriverAsync: Driver {DriverId} is not available for booking {BookingId}.", driverId, bookingId);
                return ServiceResponse<bool>.FailResponse(HttpStatusCode.BadRequest, "Driver is not available for this booking.");
            }

            booking.DriverIdFK = driverId;
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

    public async Task<ServiceResponse<List<Drivers>>> GetAvailbleDrivers(int bookingId)
    {
        try
        {
            var booking = await _bookingRepo.GetBookingByIdAsync(bookingId);
            if (booking is null)
            {
                _logger.LogWarning("GetAvailbleDrivers: Booking with ID {BookingId} not found.", bookingId);
                return ServiceResponse<List<Drivers>>.FailResponse(
                    HttpStatusCode.NotFound,
                    "Could not find the booking!");
            }

            var drivers = await _driverRepo.GetAllDriversAsync();
            var freeDrivers = new List<Drivers>();

            foreach (var driver in drivers)
            {
                var freeDriver = await DriverAvibility(driver, booking);
                if (freeDriver)
                    freeDrivers.Add(driver);
            }

            _logger.LogInformation("GetAvailbleDrivers: Found {Count} available drivers for booking {BookingId}.", freeDrivers.Count, bookingId);

            return ServiceResponse<List<Drivers>>.SuccessResponse(
                HttpStatusCode.OK,
                freeDrivers,
                "Listing all available drivers for this booking.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAvailbleDrivers: Unexpected error while retrieving available drivers for booking {BookingId}.", bookingId);
            return ServiceResponse<List<Drivers>>.FailResponse(
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

    public async Task<ServiceResponse<BookingResponseDto>> ChangeBookingById(int bookingId)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            return ServiceResponse<BookingResponseDto>.FailResponse(
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred while updating the booking.");
        }
    }

    #region Private Helpers
    private async Task<bool> DriverAvibility(Drivers driver, Bookings newBooking)
    {
        var driverLastBooking = driver.Bookings
            .Where(b => b.PickUpDateTime < newBooking.PickUpDateTime && b.Status is BookingStatus.Confirmed)
            .OrderByDescending(b => b.PickUpDateTime)
            .FirstOrDefault();

        if (driverLastBooking == null)
            return true;

        var coordinates = new List<CoordinateDto>
        {
            new CoordinateDto
            {
                Latitude = (decimal)driverLastBooking.DropOffLatitude,
                Longitude = (decimal)driverLastBooking.DropOffLongitude
            },
            new CoordinateDto
            {
                Latitude = (decimal)newBooking.PickUpLatitude,
                Longitude = (decimal)newBooking.PickUpLongitude
            }
        };

        var routeResult = await _mapService.GetRouteInfoAsync(coordinates);
        if (routeResult.Data is null)
            return false;

        var travelTime = routeResult.Data.DurationMinutes;
        var lastBookingEnd = driverLastBooking.PickUpDateTime.AddMinutes((double)driverLastBooking.DurationMinutes);
        var minutesBeforePickup = _bookingRules.MinMinutesBeforePickup;

        return lastBookingEnd.AddMinutes((double)travelTime) <= newBooking.PickUpDateTime.AddMinutes(-minutesBeforePickup);
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

    private IQueryable<Bookings> BuildAdminBookingsQuery(BookingSearchRequestDto query)
    {
        var bookings = _bookingRepo.GetAllQueryable(true);
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

    private (int Page, int PageSize, string SortBy, string SortOrder) ResolvePaginationSettings(BookingSearchRequestDto searchRequestDto)
    {
        var settings = _paginationSettings;
        int page = searchRequestDto.Page ?? settings.DefaultPage;
        int pageSize = Math.Min(searchRequestDto.PageSize ?? settings.DefaultPageSize, settings.MaxPageSize);
        string sortBy = searchRequestDto.SortBy ?? settings.SortBy;
        string sortOrder = searchRequestDto.SortOrder.ToString().ToLower();

        return (page, pageSize, sortBy, sortOrder);
    }
    #endregion
}
