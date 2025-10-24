using Microsoft.EntityFrameworkCore;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces;
using System.Text.Json;

namespace PegasusBackend.Services.Implementations
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(AppDBContext context, ILogger<IdempotencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// Check if this idempotency key has been used before
        public async Task<IdempotencyRecord?> GetExistingRecordAsync(string key)
        {
            try
            {
                // Find record with this key that hasn't expired
                var record = await _context.IdempotencyRecords
                    .Include(r => r.Booking) // Include booking for complete response
                    .FirstOrDefaultAsync(r =>
                        r.IdempotencyKey == key &&
                        r.ExpiresAt > DateTime.UtcNow);

                if (record != null)
                {
                    _logger.LogInformation(
                        "Found existing idempotency record for key: {Key}, BookingId: {BookingId}",
                        key,
                        record.BookingId);
                }

                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving idempotency record for key: {Key}", key);
                throw;
            }
        }

        /// Create a new idempotency record for a successful booking creation
        public async Task<IdempotencyRecord> CreateRecordAsync(
            string key,
            int bookingId,
            object responseData,
            int statusCode,
            int expirationHours = 24)
        {
            try
            {
                // Serialize the response data to JSON
                var serializedData = JsonSerializer.Serialize(responseData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var record = new IdempotencyRecord
                {
                    IdempotencyKey = key,
                    BookingId = bookingId,
                    ResponseData = serializedData,
                    StatusCode = statusCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
                };

                _context.IdempotencyRecords.Add(record);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Created idempotency record for key: {Key}, BookingId: {BookingId}, Expires: {ExpiresAt}",
                    key,
                    bookingId,
                    record.ExpiresAt);

                return record;
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                // This can happen in rare race condition scenarios
                // Two requests with same key arrive at exact same time
                _logger.LogWarning(
                    "Attempted to create duplicate idempotency record for key: {Key}. " +
                    "This indicates a race condition - fetching existing record instead.",
                    key);

                // Fetch and return the existing record
                var existing = await GetExistingRecordAsync(key);
                if (existing != null)
                    return existing;

                // If we still can't find it, rethrow
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating idempotency record for key: {Key}", key);
                throw;
            }
        }

        /// Clean up old expired idempotency records to keep database clean
        /// Should be called by a background job/scheduled task
        public async Task<int> CleanupExpiredRecordsAsync()
        {
            try
            {
                var expiredRecords = await _context.IdempotencyRecords
                    .Where(r => r.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                if (expiredRecords.Count == 0)
                {
                    _logger.LogInformation("No expired idempotency records to clean up");
                    return 0;
                }

                _context.IdempotencyRecords.RemoveRange(expiredRecords);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} expired idempotency records",
                    expiredRecords.Count);

                return expiredRecords.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired idempotency records");
                throw;
            }
        }
    }
}