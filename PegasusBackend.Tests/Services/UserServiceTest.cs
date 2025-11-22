using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System.Net;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models.Roles;
using PegasusBackend.Responses;

namespace PegasusBackend.Tests.Services;

public class UserServiceTest
{
    [Fact]
    public async Task GetUserByEmail_ReturnsUser_WhenEmailExists()
    {
        // Arrange 
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

        var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            null!,
            null!,
            null!,
            null!
        );

        var mockUserRepo = new Mock<IUserRepo>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockConfiguration = new Mock<IConfiguration>();

        var testUser = new User
        {
            Id = "user123",
            Email = "test@example.com",
            UserName = "testUser",
            FirstName = "Test",
            LastName = "User"
        };

        mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

        mockUserManager
            .Setup(x => x.GetRolesAsync(testUser))
            .ReturnsAsync(new List<string> { nameof(UserRoles.User) });

        var userService = new UserService(
            mockUserManager.Object,
            mockRoleManager.Object,
            mockUserRepo.Object,
            mockLogger.Object,
            mockMailjetService.Object,
            mockConfiguration.Object
        );

        // Act
        var result = await userService.GetUserByEmail("test@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.IsType<UserResponseDto>(result.Data);
        Assert.Equal(testUser.Email, result.Data.Email);
        Assert.Equal(testUser.FirstName, result.Data.FirstName);
        Assert.Equal(testUser.LastName, result.Data.LastName);
        Assert.Equal(testUser.UserName, result.Data.UserName);
        Assert.Equal("User found", result.Message);
    }

    [Fact]
    public async Task GetUserByEmail_ReturnsNotFound_WhenEmailDoesNotExist()
    {
        // Arrange 
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

        var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            null!,
            null!,
            null!,
            null!
        );

        var mockUserRepo = new Mock<IUserRepo>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var userService = new UserService(
            mockUserManager.Object,
            mockRoleManager.Object,
            mockUserRepo.Object,
            mockLogger.Object,
            mockMailjetService.Object,
            mockConfiguration.Object
        );

        // Act
        var result = await userService.GetUserByEmail("nonexistent@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Data);
        Assert.Equal("User not found", result.Message);
    }
    
    [Fact]
    public async Task ResendVerificationEmail_SendsEmail_WhenUserExists()
    {
        // Arrange 
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

        var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            null!,
            null!,
            null!,
            null!
        );

        var mockUserRepo = new Mock<IUserRepo>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockConfiguration = new Mock<IConfiguration>();

        var testUser = new User
        {
            Id = "user123",
            Email = "test@example.com",
            UserName = "testUser",
            FirstName = "Test",
            LastName = "User"
        };

        mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(testUser);

        mockUserManager
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(testUser))
            .ReturnsAsync("test-token-123");

        mockConfiguration
            .Setup(x => x["ConfirmMail:FrontendUrl"])
            .Returns("https://example.com/confirm");

        mockMailjetService
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<MailjetTemplateType>(),
                It.IsAny<AccountWelcomeRequestDto>(),
                It.IsAny<string>()))
            .ReturnsAsync(ServiceResponse<bool>.SuccessResponse(
                HttpStatusCode.OK,
                true,
                "Email sent"
            ));

        var userService = new UserService(
            mockUserManager.Object,
            mockRoleManager.Object,
            mockUserRepo.Object,
            mockLogger.Object,
            mockMailjetService.Object,
            mockConfiguration.Object
        );

        // Act
        var result = await userService.ResendVerificationEmail("test@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Data);
        Assert.Equal("Email sent successfully", result.Message);

        // Verify the email was sent
        mockMailjetService.Verify(
            x => x.SendEmailAsync(
                testUser.Email,
                MailjetTemplateType.Welcome,
                It.IsAny<AccountWelcomeRequestDto>(),
                MailjetSubjects.Welcome),
            Times.Once());

        // Verify token was generated
        mockUserManager.Verify(
            x => x.GenerateEmailConfirmationTokenAsync(testUser),
            Times.Once());
    }

    [Fact]
    public async Task ResendVerificationEmail_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange 
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

        var mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            new Mock<IRoleStore<IdentityRole>>().Object,
            null!,
            null!,
            null!,
            null!
        );

        var mockUserRepo = new Mock<IUserRepo>();
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockMailjetService = new Mock<IMailjetEmailService>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockUserManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var userService = new UserService(
            mockUserManager.Object,
            mockRoleManager.Object,
            mockUserRepo.Object,
            mockLogger.Object,
            mockMailjetService.Object,
            mockConfiguration.Object
        );

        // Act
        var result = await userService.ResendVerificationEmail("nonexistent@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(result.Data);
        Assert.Equal("User not found", result.Message);

        // Verify no email was sent
        mockMailjetService.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<MailjetTemplateType>(),
                It.IsAny<object>(),
                It.IsAny<string>()),
            Times.Never());
    }
}