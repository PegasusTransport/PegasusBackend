using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations.Base
{
    public abstract class BaseBookingService
    {
        protected readonly IBookingRepo _bookingRepo;
        protected readonly IBookingMapperService _bookingMapper;
        protected readonly IUserService _userService;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly PaginationSettings _paginationSettings;
        protected readonly IMapService _mapService;
        protected readonly ILogger _logger;
        protected readonly BookingRulesSettings _bookingRules;
        protected readonly IDriverRepo _driverRepo;

        protected BaseBookingService(
            IBookingRepo bookingRepo,
            IBookingMapperService bookingMapper,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PaginationSettings> paginationSettings,
            IMapService mapService,
            IOptions<BookingRulesSettings> bookingRules,
            ILogger logger,
            IDriverRepo driverRepo)
        {
            _bookingRepo = bookingRepo;
            _bookingMapper = bookingMapper;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _paginationSettings = paginationSettings.Value;
            _mapService = mapService;
            _logger = logger;
            _bookingRules = bookingRules.Value;
            _driverRepo = driverRepo;
        }

        protected async Task<UserResponseDto?> GetAuthenticatedUserAsync()
        {
            var result = await _userService.GetLoggedInUser(_httpContextAccessor.HttpContext);
            return result?.Data;
        }

        protected async Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetPagedBookingsResponseAsync(
            IQueryable<Bookings> bookingsQuery,
            BookingSearchRequestDto query,
            string successMessage)
        {
            try
            {
                var (currentPage, pageSize, sortBy, sortOrder) = ResolvePaginationSettings(query);

                var pagedResult = await bookingsQuery.ToPagedResultAsync(currentPage, pageSize, sortBy, sortOrder);
                var mappedResult = MapPagedResult(pagedResult);

                return ServiceResponse<PaginatedResult<BookingResponseDto>>.SuccessResponse(
                    HttpStatusCode.OK,
                    mappedResult,
                    successMessage.Replace("{count}", mappedResult.CurrentCount.ToString())
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedBookingsResponseAsync: Error while paginating bookings.");
                return ServiceResponse<PaginatedResult<BookingResponseDto>>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while retrieving bookings.");
            }
        }

        protected (int Page, int PageSize, string SortBy, string SortOrder) ResolvePaginationSettings(BookingSearchRequestDto query)
        {
            int page = query.Page ?? _paginationSettings.DefaultPage;
            int pageSize = Math.Min(query.PageSize ?? _paginationSettings.DefaultPageSize, _paginationSettings.MaxPageSize);
            string sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? _paginationSettings.SortBy : query.SortBy!;
            string sortOrder = query.SortOrder.ToString().ToLower();

            return (page, pageSize, sortBy, sortOrder);
        }
        protected IQueryable<Bookings> BuildUserBookingsQuery(string userId, BookingSearchRequestDto query)
        {
            var bookings = _bookingRepo.GetAllQueryable(true)
                .Where(b => b.UserIdFk == userId);

            return ApplyCommonFilters(bookings, query);
        }

        protected IQueryable<Bookings> BuildDriverBookingsQuery(Guid driverId, BookingSearchRequestDto query)
        {
            var bookings = _bookingRepo.GetAllQueryable(true).Where(b => b.DriverIdFK == driverId);
            return ApplyCommonFilters(bookings, query);
        }

        protected PaginatedResult<BookingResponseDto> MapPagedResult(PaginatedResult<Bookings> paged)
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

        protected async Task<bool> DriverAvailabilityAsync(Drivers driver, Bookings newBooking)
        {
            var confirmedBookings = driver.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed)
                .OrderBy(b => b.PickUpDateTime)
                .ToList();

            var bookingBefore = confirmedBookings.LastOrDefault(b => b.PickUpDateTime < newBooking.PickUpDateTime);
            var bookingAfter = confirmedBookings.FirstOrDefault(b => b.PickUpDateTime > newBooking.PickUpDateTime);

            if (bookingBefore != null && !await CanDriverTravelBetweenBookings(bookingBefore, newBooking))
                return false;

            if (bookingAfter != null && !await CanDriverTravelBetweenBookings(newBooking, bookingAfter))
                return false;

            return true;
        }

        protected async Task<bool> CanDriverTravelBetweenBookings(Bookings firstBooking, Bookings secondBooking)
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
                _logger.LogWarning("MapService API call failed while checking travel time between bookings.");
                return false;
            }

            var travelTimeNeeded = (double)routeResult.Data.DurationMinutes;
            var firstBookingEndTime = firstBooking.PickUpDateTime.AddMinutes((double)firstBooking.DurationMinutes);
            var secondBookingStartTime = secondBooking.PickUpDateTime;
            var availableTime = (secondBookingStartTime - firstBookingEndTime).TotalMinutes;
            var minutesBeforePickupBuffer = _bookingRules.MinMinutesBeforePickup;

            return availableTime >= (travelTimeNeeded + minutesBeforePickupBuffer);
        }

        protected async Task<bool> CheckDriverAvailabilityForReassignAsync(Guid driverId, Bookings newBooking)
        {
            var driver = await _driverRepo.GetDriverEntityByIdAsync(driverId);

            if (driver == null)
            {
                _logger.LogWarning("CheckDriverAvailabilityForReassignAsync: Driver {DriverId} not found.", driverId);
                return false;
            }

            if (driver.Bookings == null || !driver.Bookings.Any(b => b.Status == BookingStatus.Confirmed))
                return true;

            return await DriverAvailabilityAsync(driver, newBooking);
        }

        protected IQueryable<Bookings> ApplyCommonFilters(IQueryable<Bookings> q, BookingSearchRequestDto query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.ToLower();
                q = q.Where(b =>
                    b.PickUpAdress.ToLower().Contains(term) ||
                    b.DropOffAdress.ToLower().Contains(term) ||
                   (b.Flightnumber != null && b.Flightnumber.ToLower().Contains(term)) ||
                   (b.Comment != null && b.Comment.ToLower().Contains(term)) ||
                   // Guest name
                   (((b.GuestFirstName ?? "") + " " + (b.GuestLastName ?? "")).ToLower().Contains(term)) ||
                   // Registered user name
                   (b.User != null && (((b.User.FirstName ?? "") + " " + (b.User.LastName ?? "")).ToLower().Contains(term))) ||
                   // Driver name
                   (b.Driver != null && b.Driver.User != null &&
                    (((b.Driver.User.FirstName ?? "") + " " + (b.Driver.User.LastName ?? "")).ToLower().Contains(term)))
                );
            }
            if (query.MinPrice.HasValue) q = q
                    .Where(b => b.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue) q = q
                    .Where(b => b.Price <= query.MaxPrice.Value);

            if (query.FromDate.HasValue) q = q
                    .Where(b => b.PickUpDateTime >= query.FromDate.Value);

            if (query.ToDate.HasValue) q = q
                    .Where(b => b.PickUpDateTime <= query.ToDate.Value);

            if (query.Status.HasValue) q = q
                    .Where(b => b.Status == (BookingStatus)query.Status.Value);

            if (query.UpcomingOnly == true) q = q
                    .Where(b => b.PickUpDateTime > DateTime.UtcNow);

            return q;
        }

    }
}

