using FluentValidation;
using PegasusBackend.DTOs.TaxiDTOs;

namespace PegasusBackend.Validators.TaxiValidators
{
    public class NewTaxiSettingsValidator : AbstractValidator<NewTaxiSettingsDTO>
    {
        public NewTaxiSettingsValidator()
        {
            RuleFor(x => x.KmPrice)
                .InclusiveBetween(0, 200)
                .WithMessage("KmPrice must be between 0 and 200 SEK per km.");

            RuleFor(x => x.MinutePrice)
                .InclusiveBetween(0, 50)
                .WithMessage("MinutePrice must be between 0 and 50 SEK per minute.");

            RuleFor(x => x.StartPrice)
                .InclusiveBetween(0, 1000)
                .WithMessage("StartPrice must be between 0 and 1000 SEK.");

            RuleFor(x => x.ZonePrice)
                .InclusiveBetween(0, 2000)
                .WithMessage("ZonePrice must be between 0 and 2000 SEK.");
        }
    }
}
