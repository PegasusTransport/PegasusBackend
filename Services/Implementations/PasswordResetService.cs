using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using PegasusBackend.Configurations;
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

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(RequestPasswordResetDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Email is required"
                    );
                }
                var user = await _userManager.FindByEmailAsync(request.Email.Trim());

                if (user == null || !user.EmailConfirmed || user.IsDeleted)
                {
                    _logger.LogWarning(
                        "Password reset requested for non-existent, unconfirmed, or deleted user: {Email}",
                        request.Email
                    );

                    // Add delay to prevent timing attacks (match normal execution time)
                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    return ServiceResponse<string>.SuccessResponse(
                        HttpStatusCode.OK,
                        "If the email exists, a password reset link has been sent.",
                        "Password reset email sent"
                    );
                }

                var resetToken = await _userManager.GenerateUserTokenAsync(
                user,
                "PasswordResetTokenProvider",
                "ResetPassword"
                );

                if (string.IsNullOrEmpty(resetToken))
                {
                    _logger.LogError("Failed to generate password reset token for user: {Email}", user.Email);
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        "Failed to generate reset token"
                    );
                }

                var encodedToken = Uri.EscapeDataString(resetToken);
                var encodedEmail = Uri.EscapeDataString(user.Email!);

                var frontendUrl = _passwordResetSettings.FrontendUrl;

                if (string.IsNullOrWhiteSpace(frontendUrl))
                {
                    _logger.LogError("PasswordResetSettings:FrontendUrl configuration is missing");
                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        "Password reset configuration error"
                    );
                }

                var resetLink = $"{frontendUrl}?token={encodedToken}&email={encodedEmail}";

                var emailResult = await _mailjetEmailService.SendEmailAsync(
                    user.Email!,
                    MailjetTemplateType.ForgotPassword,
                    new DTOs.MailjetDTOs.ForgotPasswordRequestDto
                    {
                        Firstname = user.FirstName,
                        ResetLink = resetLink
                    },
                    MailjetSubjects.ForgotPassword
                );

                if (emailResult.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError(
                        "Failed to send password reset email to {Email}: {Message}",
                        user.Email,
                        emailResult.Message
                    );

                    return ServiceResponse<string>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        "Failed to send reset email"
                    );
                }

                _logger.LogInformation(
                    "Password reset email sent successfully to {Email}. Token valid for {Hours} hours.",
                    user.Email,
                    _passwordResetSettings.TokenLifetimeHours
                );

                return ServiceResponse<string>.SuccessResponse(
                    HttpStatusCode.OK,
                    "If the email exists, a password reset link has been sent.",
                    "Password reset email sent"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing forgot password request for {Email}", request.Email);
                return ServiceResponse<string>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while processing your request"
                );
            }
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ConfirmPasswordResetDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Email is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Reset token is required"
                    );
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "New password is required"
                    );
                }

                var user = await _userManager.FindByEmailAsync(request.Email.Trim());

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning(
                        "Password reset attempted for non-existent or deleted user: {Email}",
                        request.Email
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid password reset request"
                    );
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning(
                        "Password reset attempted for unconfirmed user: {Email}",
                        request.Email
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Email must be confirmed before resetting password"
                    );
                }

                string decodedToken;
                try
                {
                    decodedToken = Uri.UnescapeDataString(request.Token);

                    if (string.IsNullOrWhiteSpace(decodedToken))
                    {
                        _logger.LogWarning("Empty token after decoding for user {Email}", request.Email);
                        return ServiceResponse<bool>.FailResponse(
                            HttpStatusCode.BadRequest,
                            "Invalid token format"
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to decode token for user {Email}", request.Email);
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Invalid token format"
                    );
                }

                var isValidToken = await _userManager.VerifyUserTokenAsync(
                   user,
                   "PasswordResetTokenProvider",
                   "ResetPassword",
                   decodedToken
               );

                _logger.LogDebug(
                     "Token validation result for user {Email}: {IsValid}",
                     request.Email,
                     isValidToken
                );

                if (!isValidToken)
                {
                    _logger.LogWarning(
                        "Invalid or expired password reset token for user {Email}",
                        request.Email
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.Gone,
                        "Password reset token has expired or is invalid"
                    );
                }

                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                    _logger.LogWarning(
                        "Failed to remove old password for user {Email}: {Errors}",
                        request.Email,
                        errors
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to reset password. Please ensure your new password meets all requirements."
                    );
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                    _logger.LogWarning(
                        "Failed to set new password for user {Email}: {Errors}",
                        request.Email,
                        errors
                    );

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Failed to reset password. Please ensure your new password meets all requirements."
                    );
                }

                // This prevents token reuse - all previous reset tokens are now invalid
                try
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                    _logger.LogInformation(
                        "Security stamp updated for user {Email}, all tokens invalidated",
                        user.Email
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to update security stamp for user {Email}. Token reuse may be possible.",
                        user.Email
                    );
                    // Continue anyway - password was changed successfully
                }

                // Invalidate all refresh tokens to force re-login
                try
                {
                    await _userService.InvalidateRefreshTokenAsync(user);
                    _logger.LogInformation(
                        "Refresh tokens invalidated for user {Email}",
                        user.Email
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to invalidate refresh tokens for user {Email}. User may remain logged in.",
                        user.Email
                    );
                    // Continue anyway - password was changed successfully
                }

                _logger.LogInformation("Password successfully reset for user {Email}", user.Email);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    "Password has been reset successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error resetting password for {Email}",
                    request.Email
                );

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while resetting your password"
                );
            }
        }
    }
}