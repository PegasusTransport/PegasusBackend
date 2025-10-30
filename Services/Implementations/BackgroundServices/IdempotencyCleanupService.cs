using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.BackgroundServices
{
    public class IdempotencyCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IdempotencyCleanupService> _logger;
        private readonly IdempotencySettings _settings;

        public IdempotencyCleanupService(
            IServiceProvider serviceProvider,
            ILogger<IdempotencyCleanupService> logger,
            IOptions<IdempotencySettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Idempotency Cleanup Service started. Will run every {Hours} hours.",
                _settings.CleanupIntervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during idempotency cleanup");
                }

                // Wait for next cleanup interval
                await Task.Delay(
                    TimeSpan.FromHours(_settings.CleanupIntervalHours),
                    stoppingToken);
            }
        }

        private async Task PerformCleanupAsync()
        {
            _logger.LogInformation("Starting idempotency records cleanup");

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var idempotencyService = scope.ServiceProvider
                .GetRequiredService<IIdempotencyService>();

            var deletedCount = await idempotencyService.CleanupExpiredRecordsAsync();

            _logger.LogInformation(
                "Idempotency cleanup completed. Deleted {Count} expired records.",
                deletedCount);
        }
    }
}