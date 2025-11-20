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

        public IQueryable<Bookings> GetAllQueryable(bool includeRelations = true)
        {
            var query = _context.Bookings.AsQueryable();

            if (includeRelations)
            {
                query = query
                    .Include(b => b.User)
                    .Include(b => b.Driver)
                        .ThenInclude(d => d.User)
                     .Include(b => b.Driver)
                        .ThenInclude(d => d.Car);
            }

            return query;
        }

        public async Task<Bookings> CreateBookingAsync(Bookings booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Bookings?> GetBookingByIdAsync(int bookingId)
        {
            return await GetAllQueryable(true)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<Bookings?> GetBookingByConfirmationTokenAsync(string token)
        {
            return await GetAllQueryable(true)
                .FirstOrDefaultAsync(b => b.ConfirmationToken == token);
        }

        public async Task<List<Bookings>> GetUserBookingsAsync(string userId)
        {
            return await GetAllQueryable(true)
                .Where(b => b.UserIdFk == userId)
                .OrderByDescending(b => b.BookingDateTime)
                .ToListAsync();
        }

        public async Task<List<Bookings>> GetAvailableBookingsAsync()
        {
            return await GetAllQueryable(true)
                .Where(b => b.IsAvailable)
                .ToListAsync();
        }

        public async Task<bool> UpdateBookingAsync(Bookings booking)
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBookingAsync(Bookings booking)
        {
            _context.Bookings.Remove(booking);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
