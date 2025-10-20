using FluentValidation;
using PegasusBackend.DTOs.MapDTOs;

namespace PegasusBackend.Validators.MapValidators
{
    public class RouteInfoDtoValidator : AbstractValidator<RouteInfoDto>
    {
        public RouteInfoDtoValidator()
        {
            RuleFor(x => x.DistanceKm)
                .InclusiveBetween(0.1m, 1000m)
                .WithMessage("DistanceKm must be between 0.1 and 1000 km.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(1m, 600m)
                .WithMessage("DurationMinutes must be between 1 and 600 minutes.");

            RuleFor(x => x.Sections)
                .NotEmpty().WithMessage("At least one section is required.");
        }
    }
}
