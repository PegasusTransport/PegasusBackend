using Microsoft.AspNetCore.Identity;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMailjetEmailService _mailjetEmailService;
        private readonly IUserService _userService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IMailjetEmailService mailjetEmailService,
            IUserService userService,
            ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mailjetEmailService = mailjetEmailService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(RequestPasswordResetDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null || !user.EmailConfirmed || user.IsDeleted)
                {
                    return ServiceResponse<string>.SuccessResponse(
                        HttpStatusCode.OK,
                        "If the email exists, a password reset link has been sent.",
                        "Password reset email sent"
                    );
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(resetToken);
                var encodedEmail = Uri.EscapeDataString(user.Email!);

                var frontendUrl = _configuration["ForgotPassword:FrontendUrl"];
                var resetLink = $"{frontendUrl}?token={encodedToken}&email={encodedEmail}";

                await _mailjetEmailService.SendEmailAsync(
                    user.Email!,
                    MailjetTemplateType.ForgotPassword,
                    new DTOs.MailjetDTOs.ForgotPasswordRequestDto
                    {
                        Firstname = user.FirstName,
                        ResetLink = resetLink
                    },
                    MailjetSubjects.ForgotPassword
                );

                _logger.LogInformation("Password reset email sent to {Email}", user.Email);

                return ServiceResponse<string>.SuccessResponse(
                    HttpStatusCode.OK,
                    "If the email exists, a password reset link has been sent.",
                    "Password reset email sent"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password request");
                return ServiceResponse<string>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while processing your request"
                );
            }
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ConfirmPasswordResetDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null || user.IsDeleted)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid password reset request"
                    );
                }

                var result = await _userManager.ResetPasswordAsync(
                    user,
                    request.Token,
                    request.NewPassword
                );

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                    if (result.Errors.Any(e => e.Code == "InvalidToken"))
                    {
                        return ServiceResponse<bool>.FailResponse(
                            HttpStatusCode.Gone,
                            "Password reset token has expired or is invalid"
                        );
                    }

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        $"Failed to reset password: {errors}"
                    );
                }

                await _userService.InvalidateRefreshTokenAsync(user);
                await _userManager.UpdateSecurityStampAsync(user);

                _logger.LogInformation("Password successfully reset for user {Email}", user.Email);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Password has been reset successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while resetting your password"
                );
            }
        }
    }
}