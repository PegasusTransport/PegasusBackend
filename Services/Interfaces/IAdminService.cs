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
            BookingFilterDto? filters, BookingSearchRequestDto searchRequestDto);

        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId);

        Task<ServiceResponse<bool>> AssignDriverAsync(int bookingId, Guid driverId);

        Task<ServiceResponse<List<Drivers>>> GetAvailbleDrivers(int bookingId);

        Task<ServiceResponse<bool>> DeleteBookingByIdAsync(int bookingId);

        Task<ServiceResponse<BookingResponseDto>> ChangeBookingById(int bookingId);


    }
}
