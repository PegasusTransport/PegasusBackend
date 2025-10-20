using FluentValidation;
using PegasusBackend.DTOs.TaxiDTOs;

namespace PegasusBackend.Validators.TaxiValidators
{
    public class PriceCalculationRequestValidator : AbstractValidator<PriceCalculationRequestDto>
    {
        public PriceCalculationRequestValidator()
        {
            RuleFor(x => x.PickupAdress)
                .NotEmpty().WithMessage("PickupAdress is required.")
                .MaximumLength(200).WithMessage("PickupAdress can't exceed 200 characters.");

            RuleFor(x => x.DropoffAdress)
                .NotEmpty().WithMessage("DropoffAdress is required.")
                .MaximumLength(200).WithMessage("DropoffAdress can't exceed 200 characters.");

            RuleFor(x => x.LastDistanceKm)
                .InclusiveBetween(0.1m, 1000m)
                .WithMessage("LastDistanceKm must be between 0.1 and 1000 km.");

            RuleFor(x => x.LastDurationMinutes)
                .InclusiveBetween(1m, 600m)
                .WithMessage("LastDurationMinutes must be between 1 and 600 minutes.");

            RuleFor(x => x.FirstStopAdress)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.FirstStopAdress))
                .WithMessage("FirstStopAdress can't exceed 200 characters.");

            RuleFor(x => x.FirstStopDistanceKm)
                .InclusiveBetween(0.1m, 1000m)
                .When(x => x.FirstStopDistanceKm.HasValue)
                .WithMessage("FirstStopDistanceKm must be between 0.1 and 1000 km.");

            RuleFor(x => x.FirstStopDurationMinutes)
                .InclusiveBetween(1m, 600m)
                .When(x => x.FirstStopDurationMinutes.HasValue)
                .WithMessage("FirstStopDurationMinutes must be between 1 and 600 minutes.");

            RuleFor(x => x.SecondStopAdress)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.SecondStopAdress))
                .WithMessage("SecondStopAdress can't exceed 200 characters.");

            RuleFor(x => x.SecondStopDistanceKm)
                .InclusiveBetween(0.1m, 1000m)
                .When(x => x.SecondStopDistanceKm.HasValue)
                .WithMessage("SecondStopDistanceKm must be between 0.1 and 1000 km.");

            RuleFor(x => x.SecondStopDurationMinutes)
                .InclusiveBetween(1m, 600m)
                .When(x => x.SecondStopDurationMinutes.HasValue)
                .WithMessage("SecondStopDurationMinutes must be between 1 and 600 minutes.");
        }
    }
}
