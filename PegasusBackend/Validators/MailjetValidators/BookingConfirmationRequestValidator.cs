using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class BookingConfirmationRequestValidator : AbstractValidator<BookingConfirmationRequestDto>
    {
        public BookingConfirmationRequestValidator()
        {
            RuleFor(x => x.Firstname)
                .NotEmpty().WithMessage("Firstname is required.")
                .MaximumLength(50).WithMessage("Firstname cannot exceed 50 characters.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Pickup address is required.")
                .MaximumLength(200).WithMessage("Pickup address cannot exceed 200 characters.");

            RuleFor(x => x.Stops)
                .MaximumLength(300).WithMessage("Stops cannot exceed 300 characters.");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required.")
                .MaximumLength(200).WithMessage("Destination cannot exceed 200 characters.");

            RuleFor(x => x.PickupTime)
                .NotEmpty().WithMessage("Pickup time is required.");

            RuleFor(x => x.TotalPrice)
                .InclusiveBetween(0, 10000)
                .WithMessage("Total price must be between 0 and 10000 SEK.");
        }
    }
}
