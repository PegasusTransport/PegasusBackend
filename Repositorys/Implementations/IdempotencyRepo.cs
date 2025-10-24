using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;

namespace PegasusBackend.Repositorys.Implementations
{
    public class IdempotencyRepo : IIdempotencyRepo
    {
        private readonly AppDBContext _context;
        private readonly ILogger<IdempotencyRepo> _logger;

        public IdempotencyRepo(AppDBContext context, ILogger<IdempotencyRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IdempotencyRecord?> GetByKeyAsync(string key)
        {
            try
            {
                return await _context.IdempotencyRecords
                    .Include(r => r.Booking)
                    .FirstOrDefaultAsync(r =>
                        r.IdempotencyKey == key &&
                        r.ExpiresAt > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving idempotency record for key: {Key}", key);
                throw;
            }
        }

        public async Task<IdempotencyRecord> CreateAsync(IdempotencyRecord record)
        {
            try
            {
                _context.IdempotencyRecords.Add(record);
                await _context.SaveChangesAsync();
                return record;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                _logger.LogWarning(
                    "Attempted to create duplicate idempotency record for key: {Key}. Race condition detected.",
                    record.IdempotencyKey);

                // Fetch the existing record
                var existing = await GetByKeyAsync(record.IdempotencyKey);
                if (existing != null)
                    return existing;

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating idempotency record for key: {Key}", record.IdempotencyKey);
                throw;
            }
        }

        public async Task<int> DeleteExpiredAsync()
        {
            try
            {
                var expiredRecords = await _context.IdempotencyRecords
                    .Where(r => r.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                if (expiredRecords.Count == 0)
                    return 0;

                _context.IdempotencyRecords.RemoveRange(expiredRecords);
                await _context.SaveChangesAsync();

                return expiredRecords.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expired idempotency records");
                throw;
            }
        }
    }
}