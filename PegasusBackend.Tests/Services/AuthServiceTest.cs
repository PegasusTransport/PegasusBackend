using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PegasusBackend.Tests.Services
{
    public class AuthServiceTest
    {

        [Fact]
        public async Task SendTwoFa_SendTwoFa_WhenEmailExist()
        {
            // Arrange 
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                It.IsAny<IOptions<IdentityOptions>>(),
                It.IsAny<IPasswordHasher<User>>(),
                It.IsAny<IEnumerable<IUserValidator<User>>>(),
                It.IsAny<IEnumerable<IPasswordValidator<User>>>(),
                It.IsAny<ILookupNormalizer>(),
                It.IsAny<IdentityErrorDescriber>(),
                It.IsAny<IServiceProvider>(),
                It.IsAny<ILogger<UserManager<User>>>());

            var mockMailjetService = new Mock<IMailjetEmailService>();

            var testUser = new User
            {
                Email = "test@example.com",
                IsDeleted = false,
                EmailConfirmed = true,
                FirstName = "Test"
            };

            mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

            mockUserManager
                .Setup(p => p.CheckPasswordAsync(testUser, It.IsAny<string>())).ReturnsAsync(true);

            mockUserManager
                .Setup(l => l.IsLockedOutAsync(testUser)).ReturnsAsync(false);

            mockUserManager.Setup(t => t.GenerateTwoFactorTokenAsync(testUser, TokenOptions.DefaultEmailProvider))
                .ReturnsAsync("123456");

            mockMailjetService.Setup(e => e.SendEmailAsync(
                testUser.Email,
                MailjetTemplateType.TwoFA,
                It.IsAny<TwoFARequestDto>,
                MailjetSubjects.TwoFA
                ));

            var mockConfiguration = new Mock<IConfiguration>();
            var mockUserRepo = new Mock<IUserRepo>();
            var mockUserService = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<AuthService>>();

            var authService = new AuthService(
                mockUserManager.Object,
                mockConfiguration.Object,
                mockUserRepo.Object,
                mockUserService.Object,
                mockLogger.Object,
                mockMailjetService.Object
            );

            var loginRequest = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };
            // Act
            var result = await authService.SendTwoFaAsync(loginRequest);

            // Assert
            mockMailjetService.Verify(

                x => x.SendEmailAsync(
                    testUser.Email,
                    MailjetTemplateType.TwoFA,
                    It.IsAny<TwoFARequestDto>(),
                    MailjetSubjects.TwoFA
                ),
                Times.Once());

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.IsType<LoginResponseDto>(result.Data);
            Assert.Equal($"A verification Code has been sent to {testUser.Email}", result.Message);
        }
    }
}
