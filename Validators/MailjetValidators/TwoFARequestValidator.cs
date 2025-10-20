using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class TwoFARequestValidator : AbstractValidator<TwoFARequestDto>
    {
        public TwoFARequestValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty().WithMessage("Firstname is required.")
                .MaximumLength(50).WithMessage("Firstname cannot exceed 50 characters.");

            RuleFor(x => x.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(6).WithMessage("Verification code must be exactly 6 digits.")
                .Matches(@"^\d+$").WithMessage("Verification code must contain only numbers.");
        }
    }
}
