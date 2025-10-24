using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;
using System.Text.Json;

namespace PegasusBackend.Services.Implementations
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyRepo _repo;
        private readonly ILogger<IdempotencyService> _logger;

        public IdempotencyService(IIdempotencyRepo repo, ILogger<IdempotencyService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IdempotencyRecord?> GetExistingRecordAsync(string key)
        {
            try
            {
                var record = await _repo.GetByKeyAsync(key);

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

        public async Task<IdempotencyRecord> CreateRecordAsync(
            string key,
            int bookingId,
            object responseData,
            int statusCode,
            int expirationHours = 24)
        {
            try
            {
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

                var createdRecord = await _repo.CreateAsync(record);

                _logger.LogInformation(
                    "Created idempotency record for key: {Key}, BookingId: {BookingId}, Expires: {ExpiresAt}",
                    key,
                    bookingId,
                    createdRecord.ExpiresAt);

                return createdRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating idempotency record for key: {Key}", key);
                throw;
            }
        }

        public async Task<int> CleanupExpiredRecordsAsync()
        {
            try
            {
                var count = await _repo.DeleteExpiredAsync();

                if (count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired idempotency records", count);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired idempotency records");
                throw;
            }
        }
    }
}