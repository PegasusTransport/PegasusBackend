using FluentValidation;
using PegasusBackend.DTOs.AuthDTOs;

namespace PegasusBackend.Validators.AuthValidators
{
    public class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetDto>
    {
        public RequestPasswordResetValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(150).WithMessage("Email can't exceed 150 characters.");
        }
    }
}