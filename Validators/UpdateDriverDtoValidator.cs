using FluentValidation;
using PegasusBackend.DTOs.DriverDTO;

namespace PegasusBackend.Validators
{
    public class UpdateDriverDtoValidator : AbstractValidator<UpdateDriverDto>
    {
        public UpdateDriverDtoValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.ProfilePicture), () =>
            {
                RuleFor(x => x.ProfilePicture)
                    .MaximumLength(300).WithMessage("Profile picture URL can't exceed 300 characters.")
                    .Must(BeValidUrl).WithMessage("Profile picture must be a valid URL.");
            });

            When(x => x.CarId.HasValue, () =>
            {
                RuleFor(x => x.CarId)
                    .GreaterThan(0).WithMessage("CarId must be greater than 0.");
            });
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}