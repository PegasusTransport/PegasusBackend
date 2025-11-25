using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PegasusBackend.Configurations;
using PegasusBackend.Helpers.BookingHelpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Services.Interfaces.BookingInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PegasusBackend.Tests.Services
{
    public class CreateAdminTest
    {
        [Fact]
        public async Task CreateAdminAsync_ReturnsNotFound_WhenUserDoesNotexist()
        {
            //arrange
            var userStore = new Mock<IUserStore<User>>().Object;
            var mailjetEmailServiceMock = new Mock<IMailjetEmailService>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStore, null!, null!, null!, null!, null!, null!, null!, null!
                );
            userManagerMock
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var adminRepoMock = new Mock<IAdminRepo>();
            var loggerMock = new Mock<ILogger<AdminService>>();
            var paginationOptionsMock = new Mock<IOptions<PaginationSettings>>();
            var bookingMapperMock = new Mock<IBookingMapperService>();
            var bookingRepoMock = new Mock<IBookingRepo>();
            var driverRepoMock = new Mock<IDriverRepo>();
            var mapServiceMock = new Mock<IMapService>();
            var bookingRulesMock = new Mock<IOptions<BookingRulesSettings>>();
            var bookingValidationMock = new Mock<IBookingValidationService>();
            var recalcHelper = new RecalculateIfAddressChangedHelper(bookingValidationMock.Object);
            //needs to have mockdata for the DI used in service
            var bookingRulesSettings = new BookingRulesSettings
            {
                MinHoursBeforePickupForChange = 24
            };
            var validateHelper = new ValidateUpdateRuleHelper(bookingValidationMock.Object, bookingRulesSettings);

            var bookingServiceMock = new Mock<IBookingService>();
            var validationServiceMock = new Mock<IBookingValidationService>();
            var userServiceMock = new Mock<IUserService>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var service = new AdminService(
                adminRepoMock.Object,
                loggerMock.Object,
                paginationOptionsMock.Object,
                bookingRepoMock.Object,
                bookingMapperMock.Object,
                driverRepoMock.Object,
                mapServiceMock.Object,
                bookingRulesMock.Object,
                recalcHelper,
                validateHelper,
                bookingServiceMock.Object,
                validationServiceMock.Object,
                userServiceMock.Object,
                httpContextAccessorMock.Object,
                userManagerMock.Object,
                mailjetEmailServiceMock.Object
            );

            //act
            var result = await service.CreateAdminAsync("user@notexist.se");

            //assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Equal("User not found", result.Message);
        }
    }
}