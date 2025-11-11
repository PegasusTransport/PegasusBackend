using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<ServiceResponse<bool>> ForgotPasswordAsync(RequestPasswordResetDto request);
        Task<ServiceResponse<bool>> ResetPasswordAsync(ConfirmPasswordResetDto request);
        Task<ServiceResponse<bool>> ChangePasswordAsync(ChangePasswordDto request, HttpContext httpContext);
    }
}