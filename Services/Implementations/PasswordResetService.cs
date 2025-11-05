using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PegasusBackend.Configurations;
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
        private readonly IMailjetEmailService _mailjetEmailService;
        private readonly IUserService _userService;
        private readonly ILogger<PasswordResetService> _logger;
        private readonly PasswordResetSettings _passwordResetSettings;

        public PasswordResetService(
            UserManager<User> userManager,
            IMailjetEmailService mailjetEmailService,
            IUserService userService,
            ILogger<PasswordResetService> logger,
            IOptions<PasswordResetSettings> passwordResetSettings)
        {
            _userManager = userManager;
            _mailjetEmailService = mailjetEmailService;
            _userService = userService;
            _logger = logger;
            _passwordResetSettings = passwordResetSettings.Value;
        }

        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(RequestPasswordResetDto request)
        {
            var startTime = DateTime.UtcNow;
            const int TARGET_RESPONSE_TIME_MS = 800; 

            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email.Trim());

                if (user == null || !user.EmailConfirmed || user.IsDeleted)
                {
                    _logger.LogWarning(
                        "Password reset requested for non-existent, unconfirmed, or deleted user: {Email}",
                        request.Email
                    );
                }
                else
                {
                    var resetToken = await _userManager.GenerateUserTokenAsync(
                        user,
                        "PasswordResetTokenProvider",
                        "ResetPassword"
                    );

                    if (!string.IsNullOrEmpty(resetToken))
                    {
                        var encodedToken = Uri.EscapeDataString(resetToken);
                        var encodedEmail = Uri.EscapeDataString(user.Email!);
                        var resetLink = $"{_passwordResetSettings.FrontendUrl}?token={encodedToken}&email={encodedEmail}";

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

                        _logger.LogInformation(
                            "Password reset email sent successfully to {Email}",
                            user.Email
                        );
                    }
                }

                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var remainingDelay = TARGET_RESPONSE_TIME_MS - elapsed;

                if (remainingDelay > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(remainingDelay));
                }

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "If your email is registered, you will receive a password reset link shortly."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing forgot password request");

                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var remainingDelay = TARGET_RESPONSE_TIME_MS - elapsed;

                if (remainingDelay > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(remainingDelay));
                }

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "If your email is registered, you will receive a password reset link shortly."
                );
            }
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ConfirmPasswordResetDto request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email.Trim());

                if (user == null || user.IsDeleted || !user.EmailConfirmed)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid password reset request."
                    );
                }

                string decodedToken;
                try
                {
                    decodedToken = Uri.UnescapeDataString(request.Token);

                    if (string.IsNullOrWhiteSpace(decodedToken))
                    {
                        return ServiceResponse<bool>.FailResponse(
                            HttpStatusCode.BadRequest,
                            "Invalid reset link. Please request a new password reset."
                        );
                    }
                }
                catch
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid reset link. Please request a new password reset."
                    );
                }

                var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning(
                        "Failed to reset password for user {Email}: {Errors}",
                        request.Email,
                        errors
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        $"Password does not meet requirements: {errors}"
                    );
                }

                // Invalidate all tokens and sessions
                await _userManager.UpdateSecurityStampAsync(user);
                await _userService.InvalidateRefreshTokenAsync(user);

                _logger.LogInformation("Password successfully reset for user {Email}", user.Email);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Your password has been reset successfully. Please log in with your new password."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error resetting password for {Email}", request.Email);

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred. Please try again or contact support."
                );
            }
        }
    }
}