using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IBookingRepo
    {
        Task<Bookings> CreateBookingAsync(Bookings booking);
        Task<bool> UpdateBookingAsync(Bookings booking);
        Task<Bookings?> GetBookingByIdAsync(int bookingId);
        Task<Bookings?> GetBookingByConfirmationTokenAsync(string token);
        Task<List<Bookings>> GetUserBookingsAsync(string userId);
        Task<List<Bookings>> GetAvailableBookingsAsync();
        Task<bool> DeleteBookingAsync(Bookings booking);

        IQueryable<Bookings> GetAllQueryable(bool includeRelations = true);
    }
}
