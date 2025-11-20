using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces
{
    /// Service for handling idempotency keys to prevent duplicate bookings  
    /// from multiple requests with the same key
    public interface IIdempotencyService
    {
        /// Check if an idempotency key has been used before
        Task<IdempotencyRecord?> GetExistingRecordAsync(string key);

        /// Create a new idempotency record after successfully creating a booking
        Task<IdempotencyRecord> CreateRecordAsync(
           string key,
           int bookingId,
           object responseData,
           int statusCode);
        /// Clean up expired idempotency records (should be called periodically)
        Task<int> CleanupExpiredRecordsAsync();
    }
}