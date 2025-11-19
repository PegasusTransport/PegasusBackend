using FluentValidation;
using PegasusBackend.DTOs.MapDTOs;

namespace PegasusBackend.Validators.MapValidators
{
    public class RouteSectionDtoValidator : AbstractValidator<RouteSectionDto>
    {
        public RouteSectionDtoValidator()
        {
            RuleFor(x => x.StartAddress)
                .NotEmpty().WithMessage("StartAddress is required.")
                .MaximumLength(200).WithMessage("StartAddress can't exceed 200 characters.");

            RuleFor(x => x.EndAddress)
                .NotEmpty().WithMessage("EndAddress is required.")
                .MaximumLength(200).WithMessage("EndAddress can't exceed 200 characters.");

            RuleFor(x => x.DistanceKm)
                .InclusiveBetween(0.1m, 1000m)
                .WithMessage("DistanceKm must be between 0.1 and 1000 km.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(1m, 600m)
                .WithMessage("DurationMinutes must be between 1 and 600 minutes.");
        }
    }
}
