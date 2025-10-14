using FluentValidation;
using PegasusBackend.DTOs.BookingDTOs;

namespace PegasusBackend.Validators
{
    public class CreateBookingDTOValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingDTOValidator()
        {
            // Customer info
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(50).WithMessage("FirstName can't exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(50).WithMessage("LastName can't exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Invalid phone number format.");

            // Pickup
            RuleFor(x => x.PickUpDateTime)
                .NotEmpty().WithMessage("PickUpDateTime is required.");

            RuleFor(x => x.PickUpAddress)
                .NotEmpty().WithMessage("PickUpAddress is required.")
                .MaximumLength(300).WithMessage("PickUpAddress can't exceed 300 characters.");

            RuleFor(x => x.PickUpLatitude)
                .InclusiveBetween(-90, 90).WithMessage("PickUpLatitude must be between -90 and 90.");

            RuleFor(x => x.PickUpLongitude)
                .InclusiveBetween(-180, 180).WithMessage("PickUpLongitude must be between -180 and 180.");

            // First stop (conditional)
            When(x => !string.IsNullOrEmpty(x.FirstStopAddress), () =>
            {
                RuleFor(x => x.FirstStopAddress)
                    .MaximumLength(300).WithMessage("FirstStopAddress can't exceed 300 characters.");

                RuleFor(x => x.FirstStopLatitude)
                    .NotNull().WithMessage("FirstStopLatitude required when FirstStopAddress provided.")
                    .InclusiveBetween(-90, 90).WithMessage("FirstStopLatitude must be between -90 and 90.");

                RuleFor(x => x.FirstStopLongitude)
                    .NotNull().WithMessage("FirstStopLongitude required when FirstStopAddress provided.")
                    .InclusiveBetween(-180, 180).WithMessage("FirstStopLongitude must be between -180 and 180.");
            });

            // Second stop (conditional)
            When(x => !string.IsNullOrEmpty(x.SecondStopAddress), () =>
            {
                RuleFor(x => x.SecondStopAddress)
                    .MaximumLength(300).WithMessage("SecondStopAddress can't exceed 300 characters.");

                RuleFor(x => x.SecondStopLatitude)
                    .NotNull().WithMessage("SecondStopLatitude required when SecondStopAddress provided.")
                    .InclusiveBetween(-90, 90).WithMessage("SecondStopLatitude must be between -90 and 90.");

                RuleFor(x => x.SecondStopLongitude)
                    .NotNull().WithMessage("SecondStopLongitude required when SecondStopAddress provided.")
                    .InclusiveBetween(-180, 180).WithMessage("SecondStopLongitude must be between -180 and 180.");
            });

            // Dropoff
            RuleFor(x => x.DropOffAddress)
                .NotEmpty().WithMessage("DropOffAddress is required.")
                .MaximumLength(300).WithMessage("DropOffAddress can't exceed 300 characters.");

            RuleFor(x => x.DropOffLatitude)
                .InclusiveBetween(-90, 90).WithMessage("DropOffLatitude must be between -90 and 90.");

            RuleFor(x => x.DropOffLongitude)
                .InclusiveBetween(-180, 180).WithMessage("DropOffLongitude must be between -180 and 180.");

            // Optional fields
            When(x => !string.IsNullOrEmpty(x.Flightnumber), () =>
            {
                RuleFor(x => x.Flightnumber)
                    .MaximumLength(20).WithMessage("Flightnumber can't exceed 20 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.Comment), () =>
            {
                RuleFor(x => x.Comment)
                    .MaximumLength(500).WithMessage("Comment can't exceed 500 characters.");
            });
        }
    }
}