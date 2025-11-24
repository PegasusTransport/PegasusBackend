using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PegasusBackend.Configurations;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using System.Text.Json;

namespace PegasusBackend.Tests.Services;

public class IdempotencyServiceTest
{
    [Fact]
    public async Task GetExistingRecordAsync_ReturnsRecord_WhenKeyExists()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        var testRecord = new IdempotencyRecord
        {
            IdempotencyKey = "test-key-123",
            BookingId = 42,
            ResponseData = "{\"bookingId\":42,\"status\":\"confirmed\"}",
            StatusCode = 200,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        mockRepo
            .Setup(x => x.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(testRecord);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        // Act
        var result = await service.GetExistingRecordAsync("test-key-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testRecord.IdempotencyKey, result.IdempotencyKey);
        Assert.Equal(testRecord.BookingId, result.BookingId);
        Assert.Equal(testRecord.ResponseData, result.ResponseData);
        Assert.Equal(testRecord.StatusCode, result.StatusCode);
    }

    [Fact]
    public async Task GetExistingRecordAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        mockRepo
            .Setup(x => x.GetByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((IdempotencyRecord?)null);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        // Act
        var result = await service.GetExistingRecordAsync("nonexistent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateRecordAsync_CreatesRecord_WhenDataIsValid()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        var responseData = new
        {
            bookingId = 42,
            status = "confirmed",
            message = "Booking created successfully"
        };

        var createdRecord = new IdempotencyRecord
        {
            IdempotencyKey = "test-key-123",
            BookingId = 42,
            ResponseData = JsonSerializer.Serialize(responseData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            }),
            StatusCode = 200,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        mockRepo
            .Setup(x => x.CreateAsync(It.IsAny<IdempotencyRecord>()))
            .ReturnsAsync(createdRecord);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        // Act
        var result = await service.CreateRecordAsync(
            "test-key-123",
            42,
            responseData,
            200
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-key-123", result.IdempotencyKey);
        Assert.Equal(42, result.BookingId);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("\"bookingId\":42", result.ResponseData);

        // Verify the record was created
        mockRepo.Verify(
            x => x.CreateAsync(It.Is<IdempotencyRecord>(r =>
                r.IdempotencyKey == "test-key-123" &&
                r.BookingId == 42 &&
                r.StatusCode == 200
            )),
            Times.Once());
    }

    [Fact]
    public async Task CreateRecordAsync_SetsExpirationTime_BasedOnSettings()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 48,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        var beforeCreate = DateTime.UtcNow;

        mockRepo
            .Setup(x => x.CreateAsync(It.IsAny<IdempotencyRecord>()))
            .ReturnsAsync((IdempotencyRecord record) => record);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        var responseData = new { bookingId = 42 };

        // Act
        await service.CreateRecordAsync(
            "test-key",
            42,
            responseData,
            200
        );

        var afterCreate = DateTime.UtcNow;

        // Assert
        mockRepo.Verify(
            x => x.CreateAsync(It.Is<IdempotencyRecord>(r =>
                r.ExpiresAt >= beforeCreate.AddHours(48).AddMinutes(-1) &&
                r.ExpiresAt <= afterCreate.AddHours(48).AddMinutes(1)
            )),
            Times.Once());
    }

    [Fact]
    public async Task CleanupExpiredRecordsAsync_ReturnsCount_WhenRecordsDeleted()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        mockRepo
            .Setup(x => x.DeleteExpiredAsync())
            .ReturnsAsync(5);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        // Act
        var result = await service.CleanupExpiredRecordsAsync();

        // Assert
        Assert.Equal(5, result);

        // Verify deletion was called
        mockRepo.Verify(
            x => x.DeleteExpiredAsync(),
            Times.Once());
    }

    [Fact]
    public async Task CleanupExpiredRecordsAsync_ReturnsZero_WhenNoRecordsToDelete()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        mockRepo
            .Setup(x => x.DeleteExpiredAsync())
            .ReturnsAsync(0);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        // Act
        var result = await service.CleanupExpiredRecordsAsync();

        // Assert
        Assert.Equal(0, result);

        // Verify deletion was called
        mockRepo.Verify(
            x => x.DeleteExpiredAsync(),
            Times.Once());
    }

    [Fact]
    public async Task CreateRecordAsync_SerializesData_WithCamelCase()
    {
        // Arrange
        var mockRepo = new Mock<IIdempotencyRepo>();
        var mockLogger = new Mock<ILogger<IdempotencyService>>();
        var mockSettings = new Mock<IOptions<IdempotencySettings>>();

        var settings = new IdempotencySettings
        {
            ExpirationHours = 24,
            CleanupIntervalHours = 6
        };

        mockSettings.Setup(x => x.Value).Returns(settings);

        mockRepo
            .Setup(x => x.CreateAsync(It.IsAny<IdempotencyRecord>()))
            .ReturnsAsync((IdempotencyRecord record) => record);

        var service = new IdempotencyService(
            mockRepo.Object,
            mockLogger.Object,
            mockSettings.Object
        );

        var responseData = new
        {
            BookingId = 42,
            CustomerName = "John Doe",
            TotalPrice = 250.50
        };

        // Act
        await service.CreateRecordAsync(
            "test-key",
            42,
            responseData,
            200
        );

        // Assert
        mockRepo.Verify(
            x => x.CreateAsync(It.Is<IdempotencyRecord>(r =>
                r.ResponseData.Contains("\"bookingId\"") &&
                r.ResponseData.Contains("\"customerName\"") &&
                r.ResponseData.Contains("\"totalPrice\"")
            )),
            Times.Once());
    }
}