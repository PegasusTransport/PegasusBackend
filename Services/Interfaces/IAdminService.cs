using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.TaxiDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using System.Linq.Dynamic.Core;

namespace PegasusBackend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ServiceResponse<TaxiSettings>> GetTaxiPricesAsync();
        Task<ServiceResponse<TaxiSettings>> CreatePricesAsync(NewTaxiSettingsDTO taxiSettingsDTO);

        Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAllBookingsAsync(
            BookingFilterDto? filters, int page, int pageSize, string? sortBy = null, string sortOrder = "asc");


        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId);

        Task<ServiceResponse<PagedResult<BookingResponseDto>>> SearchBookingsAsync(string query, int page, int pageSize);

        Task<ServiceResponse<bool>> AssignDriverAsync(int bookingId, string driverId);

        Task<ServiceResponse<bool>> DeleteBookingByIdAsync(int bookingId);

        Task<ServiceResponse<BookingResponseDto>> ChangeBookingById(int bookingId);


    }
}
