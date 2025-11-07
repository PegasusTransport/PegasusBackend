using FluentValidation;
using PegasusBackend.DTOs.AuthDTOs;

namespace PegasusBackend.Validators.AuthValidators
{
    public class ConfirmPasswordResetValidator : AbstractValidator<ConfirmPasswordResetDto>
    {
        public ConfirmPasswordResetValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Reset token is required.")
                .MaximumLength(1000).WithMessage("Token format is invalid.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}