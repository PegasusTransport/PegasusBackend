using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
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
        private readonly IdempotencySettings _settings;

        public IdempotencyService(
            IIdempotencyRepo repo,
            ILogger<IdempotencyService> logger,
            IOptions<IdempotencySettings> settings)
        {
            _repo = repo;
            _logger = logger;
            _settings = settings.Value;
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
            int statusCode)
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
                    ExpiresAt = DateTime.UtcNow.AddHours(_settings.ExpirationHours) 
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
                else
                {
                    _logger.LogDebug("No expired idempotency records to clean up");
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