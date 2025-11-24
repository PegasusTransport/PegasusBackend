using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.BookingDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.DTOs.ValidationResults;
using PegasusBackend.Helpers;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Implementations.BookingServices;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System.Net;

namespace PegasusBackend.Tests.Services.BookingServices;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBookingAsync_GuestUser_ReturnsSuccess_WithPendingConfirmation()
    {
        // Arrange
        var mockBookingRepo = new Mock<IBookingRepo>();
        var mockUserManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockValidationService = new Mock<IBookingValidationService>();
        var mockBookingFactory = new Mock<IBookingFactoryService>();
        var mockBookingMapper = new Mock<IBookingMapperService>();
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockMapService = new Mock<IMapService>();
        var mockDriverRepo = new Mock<IDriverRepo>();

        var mailJetSettings = Options.Create(new MailJetSettings
        {
            Links = new MailjetLinks
            {
                LocalConfirmationBase = "http://localhost:5000/confirm/",
                ProductionConfirmationBase = "https://pegasus.se/confirm/",
                DriverPortalUrl = "https://driver.pegasus.se"
            }
        });

        var bookingRulesSettings = Options.Create(new BookingRulesSettings
        {
            MinHoursBeforePickupForChange = 2
        });

        var paginationSettings = Options.Create(new PaginationSettings
        {
            DefaultPageSize = 10,
            MaxPageSize = 100
        });

        var recalculateHelper = new RecalculateIfAddressChangedHelper(mockValidationService.Object);
        var validateUpdateRuleHelper = new ValidateUpdateRuleHelper(
            mockValidationService.Object,
            bookingRulesSettings.Value
        );

        var bookingDto = new CreateBookingDto
        {
            Email = "guest@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Flightnumber = "SK123"
        };

        var validationResult = new ValidationResult
        {
            IsValid = true,
            RouteInfo = new RouteInfoDto
            {
                DistanceKm = 42.5m,
                DurationMinutes = 35,
                Sections = new List<RouteSectionDto>()
            },
            CalculatedPrice = 450.00m
        };

        var booking = new Bookings
        {
            BookingId = 1,
            UserIdFk = null,
            GuestEmail = "guest@example.com",
            GuestFirstName = "John",
            GuestLastName = "Doe",
            GuestPhoneNumber = "0701234567",
            Price = 450.00m,
            BookingDateTime = DateTime.UtcNow,
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            PickUpAdress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAdress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Status = BookingStatus.PendingEmailConfirmation,
            ConfirmationToken = "abc123token",
            IsConfirmed = false,
            IsAvailable = false,
            DistanceKm = 42.5m,
            DurationMinutes = 35
        };

        var responseDto = new BookingResponseDto
        {
            BookingId = 1,
            Email = "guest@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            IsGuestBooking = true,
            Price = 450.00m,
            Status = BookingStatus.PendingEmailConfirmation,
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            DistanceKm = 42.5m,
            DurationMinutes = 35,
            BookingDateTime = DateTime.UtcNow,
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            IsConfirmed = false
        };

        mockValidationService
            .Setup(x => x.ValidateBookingAsync(It.IsAny<CreateBookingDto>()))
            .ReturnsAsync(validationResult);

        mockUserManager
            .Setup(x => x.FindByEmailAsync(bookingDto.Email))
            .ReturnsAsync((User?)null);

        mockBookingFactory
            .Setup(x => x.CreateBookingEntity(
                It.IsAny<CreateBookingDto>(),
                It.IsAny<RouteInfoDto>(),
                It.IsAny<decimal>(),
                It.IsAny<User?>(),
                true))
            .Returns(booking);

        mockBookingRepo
            .Setup(x => x.CreateBookingAsync(It.IsAny<Bookings>()))
            .ReturnsAsync(booking);

        mockMailjetService
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<Helpers.MailjetHelpers.MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Email sent"));

        mockBookingMapper
            .Setup(x => x.MapToResponseDTO(It.IsAny<Bookings>()))
            .Returns(responseDto);

        mockEnv
            .Setup(x => x.EnvironmentName)
            .Returns("Development");

        var bookingService = new BookingService(
            mockBookingRepo.Object,
            mockUserManager.Object,
            mockMailjetService.Object,
            mockValidationService.Object,
            mockBookingFactory.Object,
            mockBookingMapper.Object,
            mockLogger.Object,
            mailJetSettings,
            bookingRulesSettings,
            mockEnv.Object,
            mockUserService.Object,
            mockHttpContextAccessor.Object,
            paginationSettings,
            recalculateHelper,
            validateUpdateRuleHelper,
            mockMapService.Object,
            mockDriverRepo.Object
        );

        // Act
        var result = await bookingService.CreateBookingAsync(bookingDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsGuestBooking);
        Assert.Equal(BookingStatus.PendingEmailConfirmation, result.Data.Status);
        Assert.Contains("confirm via email", result.Message);

        mockValidationService.Verify(
            x => x.ValidateBookingAsync(bookingDto),
            Times.Once());

        mockUserManager.Verify(
            x => x.FindByEmailAsync(bookingDto.Email),
            Times.Once());

        mockBookingRepo.Verify(
            x => x.CreateBookingAsync(It.IsAny<Bookings>()),
            Times.Once());

        mockMailjetService.Verify(
            x => x.SendEmailAsync(
                bookingDto.Email,
                It.IsAny<Helpers.MailjetHelpers.MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Once());

        mockDriverRepo.Verify(
            x => x.GetAllDriversAsync(),
            Times.Never());
    }

    [Fact]
    public async Task CreateBookingAsync_RegisteredUser_ReturnsSuccess_AndNotifiesDrivers()
    {
        // Arrange
        var mockBookingRepo = new Mock<IBookingRepo>();
        var mockUserManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockValidationService = new Mock<IBookingValidationService>();
        var mockBookingFactory = new Mock<IBookingFactoryService>();
        var mockBookingMapper = new Mock<IBookingMapperService>();
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockMapService = new Mock<IMapService>();
        var mockDriverRepo = new Mock<IDriverRepo>();

        var mailJetSettings = Options.Create(new MailJetSettings
        {
            Links = new MailjetLinks
            {
                LocalConfirmationBase = "http://localhost:5000/confirm/",
                ProductionConfirmationBase = "https://pegasus.se/confirm/",
                DriverPortalUrl = "https://driver.pegasus.se"
            }
        });

        var bookingRulesSettings = Options.Create(new BookingRulesSettings
        {
            MinHoursBeforePickupForChange = 2
        });

        var paginationSettings = Options.Create(new PaginationSettings
        {
            DefaultPageSize = 10,
            MaxPageSize = 100
        });

        var recalculateHelper = new RecalculateIfAddressChangedHelper(mockValidationService.Object);
        var validateUpdateRuleHelper = new ValidateUpdateRuleHelper(
            mockValidationService.Object,
            bookingRulesSettings.Value
        );

        var bookingDto = new CreateBookingDto
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Flightnumber = "SK123"
        };

        var testUser = new User
        {
            Id = "user123",
            Email = "user@example.com",
            UserName = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567"
        };

        var validationResult = new ValidationResult
        {
            IsValid = true,
            RouteInfo = new RouteInfoDto
            {
                DistanceKm = 42.5m,
                DurationMinutes = 35,
                Sections = new List<RouteSectionDto>()
            },
            CalculatedPrice = 450.00m
        };

        var booking = new Bookings
        {
            BookingId = 1,
            UserIdFk = "user123",
            Price = 450.00m,
            BookingDateTime = DateTime.UtcNow,
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            PickUpAdress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAdress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Status = BookingStatus.Confirmed,
            IsConfirmed = true,
            IsAvailable = true,
            DistanceKm = 42.5m,
            DurationMinutes = 35
        };

        var responseDto = new BookingResponseDto
        {
            BookingId = 1,
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            IsGuestBooking = false,
            Price = 450.00m,
            Status = BookingStatus.Confirmed,
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            DistanceKm = 42.5m,
            DurationMinutes = 35,
            BookingDateTime = DateTime.UtcNow,
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            IsConfirmed = true
        };

        var mockDrivers = new List<Drivers>
        {
            new Drivers
            {
                DriverId = Guid.NewGuid(),
                UserId = "driver1-user-id",
                User = new User
                {
                    Id = "driver1-user-id",
                    Email = "driver1@pegasus.se",
                    UserName = "driver1@pegasus.se",
                    FirstName = "Erik",
                    LastName = "Andersson"
                },
                ProfilePicture = "https://example.com/erik.jpg",
                IsDeleted = false
            },
            new Drivers
            {
                DriverId = Guid.NewGuid(),
                UserId = "driver2-user-id",
                User = new User
                {
                    Id = "driver2-user-id",
                    Email = "driver2@pegasus.se",
                    UserName = "driver2@pegasus.se",
                    FirstName = "Anna",
                    LastName = "Svensson"
                },
                ProfilePicture = "https://example.com/anna.jpg",
                IsDeleted = false
            }
        };

        mockValidationService
            .Setup(x => x.ValidateBookingAsync(It.IsAny<CreateBookingDto>()))
            .ReturnsAsync(validationResult);

        mockUserManager
            .Setup(x => x.FindByEmailAsync(bookingDto.Email))
            .ReturnsAsync(testUser);

        mockBookingFactory
            .Setup(x => x.CreateBookingEntity(
                It.IsAny<CreateBookingDto>(),
                It.IsAny<RouteInfoDto>(),
                It.IsAny<decimal>(),
                It.IsAny<User?>(),
                false))
            .Returns(booking);

        mockBookingRepo
            .Setup(x => x.CreateBookingAsync(It.IsAny<Bookings>()))
            .ReturnsAsync(booking);

        mockMailjetService
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<Helpers.MailjetHelpers.MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK, true, "Email sent"));

        mockDriverRepo
            .Setup(x => x.GetAllDriversAsync())
            .ReturnsAsync(mockDrivers);

        mockBookingMapper
            .Setup(x => x.MapToResponseDTO(It.IsAny<Bookings>()))
            .Returns(responseDto);

        mockEnv
            .Setup(x => x.EnvironmentName)
            .Returns("Development");

        var bookingService = new BookingService(
            mockBookingRepo.Object,
            mockUserManager.Object,
            mockMailjetService.Object,
            mockValidationService.Object,
            mockBookingFactory.Object,
            mockBookingMapper.Object,
            mockLogger.Object,
            mailJetSettings,
            bookingRulesSettings,
            mockEnv.Object,
            mockUserService.Object,
            mockHttpContextAccessor.Object,
            paginationSettings,
            recalculateHelper,
            validateUpdateRuleHelper,
            mockMapService.Object,
            mockDriverRepo.Object
        );

        // Act
        var result = await bookingService.CreateBookingAsync(bookingDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsGuestBooking);
        Assert.Equal(BookingStatus.Confirmed, result.Data.Status);
        Assert.Contains("confirmed successfully", result.Message);

        mockBookingRepo.Verify(
            x => x.CreateBookingAsync(It.IsAny<Bookings>()),
            Times.Once());

        mockMailjetService.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<Helpers.MailjetHelpers.MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Exactly(3));

        mockDriverRepo.Verify(
            x => x.GetAllDriversAsync(),
            Times.Once());
    }

    [Fact]
    public async Task CreateBookingAsync_ValidationFails_ReturnsError()
    {
        // Arrange
        var mockBookingRepo = new Mock<IBookingRepo>();
        var mockUserManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockValidationService = new Mock<IBookingValidationService>();
        var mockBookingFactory = new Mock<IBookingFactoryService>();
        var mockBookingMapper = new Mock<IBookingMapperService>();
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockMapService = new Mock<IMapService>();
        var mockDriverRepo = new Mock<IDriverRepo>();

        var mailJetSettings = Options.Create(new MailJetSettings
        {
            Links = new MailjetLinks
            {
                LocalConfirmationBase = "http://localhost:5000/confirm/",
                ProductionConfirmationBase = "https://pegasus.se/confirm/",
                DriverPortalUrl = "https://driver.pegasus.se"
            }
        });

        var bookingRulesSettings = Options.Create(new BookingRulesSettings
        {
            MinHoursBeforePickupForChange = 2
        });

        var paginationSettings = Options.Create(new PaginationSettings
        {
            DefaultPageSize = 10,
            MaxPageSize = 100
        });

        var recalculateHelper = new RecalculateIfAddressChangedHelper(mockValidationService.Object);
        var validateUpdateRuleHelper = new ValidateUpdateRuleHelper(
            mockValidationService.Object,
            bookingRulesSettings.Value
        );

        var bookingDto = new CreateBookingDto
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            PickUpDateTime = DateTime.UtcNow.AddHours(12),
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Flightnumber = "SK123"
        };

        var failedValidation = new ValidationResult
        {
            IsValid = false,
            ErrorResponse = ServiceResponse<BookingResponseDto>.FailResponse(
                HttpStatusCode.BadRequest,
                "PickUpDateTime must be at least 24 hours from now.")
        };

        mockValidationService
            .Setup(x => x.ValidateBookingAsync(It.IsAny<CreateBookingDto>()))
            .ReturnsAsync(failedValidation);

        var bookingService = new BookingService(
            mockBookingRepo.Object,
            mockUserManager.Object,
            mockMailjetService.Object,
            mockValidationService.Object,
            mockBookingFactory.Object,
            mockBookingMapper.Object,
            mockLogger.Object,
            mailJetSettings,
            bookingRulesSettings,
            mockEnv.Object,
            mockUserService.Object,
            mockHttpContextAccessor.Object,
            paginationSettings,
            recalculateHelper,
            validateUpdateRuleHelper,
            mockMapService.Object,
            mockDriverRepo.Object
        );

        // Act
        var result = await bookingService.CreateBookingAsync(bookingDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains("24 hours", result.Message);

        mockUserManager.Verify(
            x => x.FindByEmailAsync(It.IsAny<string>()),
            Times.Never());

        mockBookingRepo.Verify(
            x => x.CreateBookingAsync(It.IsAny<Bookings>()),
            Times.Never());

        mockMailjetService.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<Helpers.MailjetHelpers.MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Never());
    }

    [Fact]
    public async Task CreateBookingAsync_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var mockBookingRepo = new Mock<IBookingRepo>();
        var mockUserManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockValidationService = new Mock<IBookingValidationService>();
        var mockBookingFactory = new Mock<IBookingFactoryService>();
        var mockBookingMapper = new Mock<IBookingMapperService>();
        var mockLogger = new Mock<ILogger<BookingService>>();
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockMapService = new Mock<IMapService>();
        var mockDriverRepo = new Mock<IDriverRepo>();

        var mailJetSettings = Options.Create(new MailJetSettings
        {
            Links = new MailjetLinks
            {
                LocalConfirmationBase = "http://localhost:5000/confirm/",
                ProductionConfirmationBase = "https://pegasus.se/confirm/",
                DriverPortalUrl = "https://driver.pegasus.se"
            }
        });

        var bookingRulesSettings = Options.Create(new BookingRulesSettings
        {
            MinHoursBeforePickupForChange = 2
        });

        var paginationSettings = Options.Create(new PaginationSettings
        {
            DefaultPageSize = 10,
            MaxPageSize = 100
        });

        var recalculateHelper = new RecalculateIfAddressChangedHelper(mockValidationService.Object);
        var validateUpdateRuleHelper = new ValidateUpdateRuleHelper(
            mockValidationService.Object,
            bookingRulesSettings.Value
        );

        var bookingDto = new CreateBookingDto
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "0701234567",
            PickUpDateTime = DateTime.UtcNow.AddHours(48),
            PickUpAddress = "Arlanda Airport",
            PickUpLatitude = 59.6519,
            PickUpLongitude = 17.9186,
            DropOffAddress = "Stockholm Central",
            DropOffLatitude = 59.3293,
            DropOffLongitude = 18.0686,
            Flightnumber = "SK123"
        };

        mockValidationService
            .Setup(x => x.ValidateBookingAsync(It.IsAny<CreateBookingDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var bookingService = new BookingService(
            mockBookingRepo.Object,
            mockUserManager.Object,
            mockMailjetService.Object,
            mockValidationService.Object,
            mockBookingFactory.Object,
            mockBookingMapper.Object,
            mockLogger.Object,
            mailJetSettings,
            bookingRulesSettings,
            mockEnv.Object,
            mockUserService.Object,
            mockHttpContextAccessor.Object,
            paginationSettings,
            recalculateHelper,
            validateUpdateRuleHelper,
            mockMapService.Object,
            mockDriverRepo.Object
        );

        // Act
        var result = await bookingService.CreateBookingAsync(bookingDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        Assert.Contains("Something went wrong while creating booking", result.Message);
    }
}