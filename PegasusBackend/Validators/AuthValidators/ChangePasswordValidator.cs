using FluentValidation;
using PegasusBackend.DTOs.AuthDTOs;

namespace PegasusBackend.Validators.AuthValidators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.")
                .MinimumLength(6).WithMessage("Current password must be at least 6 characters.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Password confirmation is required.")
                .Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match.");
        }
    }
}