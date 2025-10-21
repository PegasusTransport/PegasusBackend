using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validations
{
    public class ReceiptRequestValidator : AbstractValidator<ReceiptRequestDto>
    {
        public ReceiptRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("BookingId must be greater than 0.");

            RuleFor(x => x.CustomerFirstname)
                .NotEmpty().WithMessage("Customer first name is required.")
                .MaximumLength(50).WithMessage("Customer first name must not exceed 50 characters.");

            RuleFor(x => x.DriverFirstname)
                .NotEmpty().WithMessage("Driver first name is required.")
                .MaximumLength(50).WithMessage("Driver first name must not exceed 50 characters.");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("License plate is required.")
                .MaximumLength(10).WithMessage("License plate must not exceed 10 characters.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Pickup address is required.")
                .MaximumLength(100).WithMessage("Pickup address must not exceed 100 characters.");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination address is required.")
                .MaximumLength(100).WithMessage("Destination must not exceed 100 characters.");

            RuleFor(x => x.PickupTime)
                .NotEmpty().WithMessage("Pickup time is required.")
                .Must(BeAValidPickupTime).WithMessage("Pickup time must be within a reasonable range.");

            RuleFor(x => x.DistanceKm)
                .GreaterThan(0).WithMessage("Distance must be greater than 0 km.")
                .LessThanOrEqualTo(300).WithMessage("Distance seems unrealistic (>300 km).");

            RuleFor(x => x.DurationMinutes)
                .NotEmpty().WithMessage("Trip duration is required.")
                .Matches(@"^\d+$").WithMessage("Trip duration must be a whole number (minutes).");

            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Total price cannot be negative.");

            RuleFor(x => x.DriverImageUrl)
                .Must(BeAValidUrlOrNull).WithMessage("Driver image URL is invalid.")
                .When(x => !string.IsNullOrEmpty(x.DriverImageUrl));
        }

        private bool BeAValidPickupTime(DateTime pickupTime)
        {
            var now = DateTime.Now;
            return pickupTime >= now.AddDays(-1) && pickupTime <= now.AddDays(3);
        }

        private bool BeAValidUrlOrNull(string? url)
        {
            return string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
