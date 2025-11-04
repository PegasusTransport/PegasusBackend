using FluentValidation;
using PegasusBackend.DTOs.DriverDTO;

public class UpdateRequestDriverDtoValidator : AbstractValidator<UpdateRequestDriverDto>
{
    public UpdateRequestDriverDtoValidator()
    {
        RuleFor(x => x.ProfilePicture)
            .NotEmpty()
            .WithMessage("Profile picture is required.")
            .MaximumLength(500)
            .WithMessage("Profile picture URL cannot exceed 500 characters.");

        RuleFor(x => x.CarId)
            .GreaterThan(0)
            .When(x => x.CarId.HasValue)
            .WithMessage("CarId must be greater than 0 when provided.");
    }
}
