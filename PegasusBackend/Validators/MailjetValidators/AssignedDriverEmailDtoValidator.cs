using FluentValidation;
using PegasusBackend.DTOs.MailjetDTOs;

public class AssignedDriverEmailDtoValidator : AbstractValidator<AssignedDriverEmailDto>
{
    public AssignedDriverEmailDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50);

        RuleFor(x => x.PickupAddress)
            .NotEmpty().WithMessage("Pickup address is required.")
            .MaximumLength(200);

        RuleFor(x => x.Stops)
            .NotNull().WithMessage("Stops cannot be null.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MaximumLength(200);

        RuleFor(x => x.TotalPrice)
            .GreaterThan(0).WithMessage("Total price must be greater than zero.");

        // Driver details
        RuleFor(x => x.DriverName)
            .NotEmpty().WithMessage("Driver name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DriverNumber)
            .NotEmpty().WithMessage("Driver phone number is required.")
            .Matches(@"^\+?\d{6,15}$").WithMessage("Driver phone number format is invalid.");

        RuleFor(x => x.LicensePlate)
            .NotEmpty().WithMessage("License plate is required.")
            .MaximumLength(20);
    }
}
