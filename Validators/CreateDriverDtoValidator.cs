using FluentValidation;
using PegasusBackend.DTOs.DriverDTO;

namespace PegasusBackend.Validators
{
    public class CreateDriverDtoValidator : AbstractValidator<CreateRequestDriverDto>
    {
        public CreateDriverDtoValidator()
        {
            RuleFor(x => x.ProfilePicture)
                .NotEmpty().WithMessage("Profile picture is required.")
                .MaximumLength(300).WithMessage("Profile picture URL can't exceed 300 characters.")
                .Must(BeValidUrl).WithMessage("Profile picture must be a valid URL.");

            RuleFor(x => x.LicensePlate)
                .MaximumLength(10).WithMessage("LicensePlate cant be longer then 10");
        }

        private bool BeValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}