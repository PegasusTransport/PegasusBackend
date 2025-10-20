using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.MailjetHelpers;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class AccountWelcomeRequestValidator : AbstractValidator<AccountWelcomeRequestDto>
    {
        public AccountWelcomeRequestValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty().WithMessage("First name is required for the email.")
                .MaximumLength(50).WithMessage("The name cannot be longer than 50 characters.");

            RuleFor(x => x.VerificationLink)
                .NotEmpty().WithMessage("Verification link is required.")
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out var uriResult) && uriResult.Scheme == Uri.UriSchemeHttps)
                .WithMessage("Verification link must start with 'https://' and be a valid URL.");

            RuleFor(x => x.ButtonName)
                 .NotEmpty().WithMessage("Button name is required.")
                 .Must(name => name == MailjetButtonType.Login || name == MailjetButtonType.Verify)
                 .WithMessage("Button name must be a valid MailjetButtonType value.");

        }
    }
}
