using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.DriverDTO;
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
        Task<ServiceResponse<PaginatedResult<BookingResponseDto>>> GetAllBookingsAsync(BookingFilterRequestForAdminDto filterRequest);
        Task<ServiceResponse<BookingResponseDto>> GetBookingByIdAsync(int bookingId);
        Task<ServiceResponse<bool>> AssignDriverAsync(int bookingId, Guid driverId);
        Task<ServiceResponse<List<AvailableDriverResponsDto>>> GetAvailbleDrivers(int bookingId);
        Task<ServiceResponse<bool>> DeleteBookingByIdAsync(int bookingId);
        Task<ServiceResponse<BookingResponseDto>> ChangeBookingById(UpdateBookingDto updateBookingDto);
        Task<ServiceResponse<bool>> CancelBookingAsync(int bookingId);
        Task<ServiceResponse<List<AllDriversRequestDto>>> GetAllDriversAsync();
        Task<ServiceResponse<DriverResponseDto>> GetDriverByUserIdAsync(string userId);
        Task<ServiceResponse<DriverResponseDto>> GetDriverByIdAsync(Guid driverId);
        Task<ServiceResponse<bool>> DeleteDriverAsync(Guid driverId);
    }
}
