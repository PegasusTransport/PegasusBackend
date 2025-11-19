using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Validators.MailjetValidators
{
    public class DriverNewBookingNotificationValidator : AbstractValidator<DriverNewBookingNotificationDto>
    {
        public DriverNewBookingNotificationValidator()
        {
            RuleFor(x => x.DriverFirstname)
                .NotEmpty().WithMessage("Driver firstname is required.")
                .MaximumLength(50).WithMessage("Driver firstname cannot exceed 50 characters.");

            RuleFor(x => x.PickupAddress)
                .NotEmpty().WithMessage("Pickup address is required.")
                .MaximumLength(200).WithMessage("Pickup address cannot exceed 200 characters.");

            RuleFor(x => x.PickupTime)
                .NotEmpty().WithMessage("Pickup time is required.")
                .MaximumLength(100).WithMessage("Pickup time format is too long.");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required.")
                .MaximumLength(200).WithMessage("Destination cannot exceed 200 characters.");

            RuleFor(x => x.Stops)
                .MaximumLength(300).WithMessage("Stops text cannot exceed 300 characters.")
                .When(x => !string.IsNullOrEmpty(x.Stops));

            RuleFor(x => x.EstimatedPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Estimated price cannot be negative.")
                .LessThanOrEqualTo(50000).WithMessage("Estimated price seems unrealistic (>50000 SEK).");

            RuleFor(x => x.DistanceKm)
                .GreaterThanOrEqualTo(0).WithMessage("Distance cannot be negative.")
                .LessThanOrEqualTo(500).WithMessage("Distance seems unrealistic (>500 km).");

            RuleFor(x => x.PortalLink)
                .NotEmpty().WithMessage("Portal link is required.")
                .Must(link => Uri.TryCreate(link, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Portal link must be a valid HTTP or HTTPS URL.");
        }
    }
}