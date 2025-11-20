using FluentValidation;
using PegasusBackend.DTOs.BookingDTOs;

namespace PegasusBackend.Validators.BookingValidators
{
    public class UpdateBookingDtoValidator : AbstractValidator<UpdateBookingDto>
    {
        public UpdateBookingDtoValidator()
        {
            RuleFor(x => x.PickUpDateTime)
                .NotEmpty().WithMessage("Pickup time is required.")
                .Must(BeAtLeast48HoursAhead)
                .WithMessage("Pickup time must be at least 24 hours from now. You cannot Change The Pickuptime if its less then 24 hours to your booking!");

            RuleFor(x => x.Flightnumber)
                .Matches(@"^[A-Z]{2}\d{1,4}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Flightnumber))
                .WithMessage("Invalid flight number format. Example: SK123 or DY456.");

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("Comment cannot exceed 500 characters.");

            When(x => !string.IsNullOrWhiteSpace(x.FirstStopAddress), () =>
            {
                RuleFor(x => x.FirstStopLatitude)
                    .NotNull().WithMessage("Latitude for the first stop is required when an address is provided.");

                RuleFor(x => x.FirstStopLongitude)
                    .NotNull().WithMessage("Longitude for the first stop is required when an address is provided.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.SecondStopAddress), () =>
            {
                RuleFor(x => x.SecondStopLatitude)
                    .NotNull().WithMessage("Latitude for the second stop is required when an address is provided.");

                RuleFor(x => x.SecondStopLongitude)
                    .NotNull().WithMessage("Longitude for the second stop is required when an address is provided.");
            });
        }

        private bool BeAtLeast48HoursAhead(DateTime pickupTime)
        {
            return pickupTime >= DateTime.UtcNow.AddHours(24);
        }
    }
}
