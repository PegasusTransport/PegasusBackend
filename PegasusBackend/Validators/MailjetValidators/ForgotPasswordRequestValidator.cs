using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty().WithMessage("Firstname is required.")
                .MaximumLength(50).WithMessage("Firstname cannot exceed 50 characters.");

            RuleFor(x => x.ResetLink)
                .NotEmpty().WithMessage("Reset link is required.")
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                .WithMessage("Reset link must be a valid HTTPS URL.");
        }
    }
}
