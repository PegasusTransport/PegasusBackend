using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class PendingConfirmationRequestValidator : AbstractValidator<PendingConfirmationRequestDto>
    {
        public PendingConfirmationRequestValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty().WithMessage("Firstname is required.")
                .MaximumLength(50).WithMessage("Firstname cannot exceed 50 characters.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Pickup address is required.")
                .MaximumLength(200).WithMessage("Pickup address cannot exceed 200 characters.");

            RuleFor(x => x.Stops)
                .MaximumLength(300).WithMessage("Stops text cannot exceed 300 characters.");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required.")
                .MaximumLength(200).WithMessage("Destination cannot exceed 200 characters.");

            RuleFor(x => x.PickupTime)
                .NotEmpty().WithMessage("Pickup time is required.")
                .MaximumLength(100).WithMessage("Pickup time format is too long.");

            RuleFor(x => x.TotalPrice)
                .InclusiveBetween(0, 10000)
                .WithMessage("Total price must be between 0 and 10000 SEK.");

            RuleFor(x => x.ConfirmationLink)
                .NotEmpty().WithMessage("Confirmation link is required.")
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                .WithMessage("Confirmation link must start with 'https://' and be valid.");
        }
    }
}
