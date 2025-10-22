using FluentValidation;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.DTOs.BookingDTOs;

namespace PegasusBackend.Validators.AuthValidators
{
    public class VerifyTwoFAValidator : AbstractValidator<Verify2FaDto>
    {
        public VerifyTwoFAValidator()
        {
            RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email is required.")
               .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.");

            RuleFor(x => x.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(6).WithMessage("Verification code must be exactly 6 digits.")
                .Matches(@"^\d+$").WithMessage("Verification code must contain only numbers.");
        }
    }
}
