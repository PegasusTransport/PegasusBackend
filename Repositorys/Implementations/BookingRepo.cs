using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class BookingRepo : IBookingRepo
    {
        private readonly AppDBContext _context;

        public BookingRepo(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Bookings> CreateBookingAsync(Bookings booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Bookings?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Driver)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<Bookings?> GetBookingByConfirmationTokenAsync(string token)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.ConfirmationToken == token);
        }

        public async Task<List<Bookings>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Driver)
                .Where(b => b.UserIdFk == userId)
                .OrderByDescending(b => b.BookingDateTime)
                .ToListAsync();
        }

        public async Task<List<Bookings>> GetAvailableBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.IsAvailable
                    && b.IsConfirmed
                    && b.Status == BookingStatus.Confirmed
                    && b.DriverIdFK == null)  // Not yet assigned to driver
                .OrderBy(b => b.PickUpDateTime)
                .ToListAsync();
        }

        public async Task<bool> UpdateBookingAsync(Bookings booking)
        {
            _context.Bookings.Update(booking);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return false;

            _context.Bookings.Remove(booking);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}