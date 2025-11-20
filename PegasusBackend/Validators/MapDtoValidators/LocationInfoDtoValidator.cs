using FluentValidation;
using PegasusBackend.DTOs.MapDTOs;

namespace PegasusBackend.Validators.MapValidators
{
    public class LocationInfoDtoValidator : AbstractValidator<LocationInfoDto>
    {
        public LocationInfoDtoValidator()
        {
            RuleFor(x => x.FormattedAddress)
                .NotEmpty().WithMessage("FormattedAddress is required.")
                .MaximumLength(200).WithMessage("FormattedAddress can't exceed 200 characters.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100).WithMessage("City can't exceed 100 characters.");

            RuleFor(x => x.Municipality)
                .NotEmpty().WithMessage("Municipality is required.")
                .MaximumLength(100).WithMessage("Municipality can't exceed 100 characters.");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("PostalCode is required.")
                .Matches(@"^\d{3}\s?\d{2}$").WithMessage("PostalCode must be in the format 12345 or 123 45.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        }
    }
}
