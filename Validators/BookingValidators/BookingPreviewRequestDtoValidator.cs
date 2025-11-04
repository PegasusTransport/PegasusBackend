using FluentValidation;
using PegasusBackend.DTOs.BookingDTOs;

namespace PegasusBackend.Validators.BookingValidators
{
    public class BookingPreviewRequestDtoValidator : AbstractValidator<BookingPreviewRequestDto>
    {
        public BookingPreviewRequestDtoValidator()
        {
            // Pickup
            RuleFor(x => x.PickUpDateTime)
                .NotEmpty().WithMessage("PickUpDateTime är obligatoriskt.");

            RuleFor(x => x.PickUpAddress)
                .NotEmpty().WithMessage("PickUpAddress är obligatoriskt.")
                .MaximumLength(300).WithMessage("PickUpAddress får inte överstiga 300 tecken.");

            RuleFor(x => x.PickUpLatitude)
                .InclusiveBetween(-90, 90).WithMessage("PickUpLatitude måste vara mellan -90 och 90.");

            RuleFor(x => x.PickUpLongitude)
                .InclusiveBetween(-180, 180).WithMessage("PickUpLongitude måste vara mellan -180 och 180.");

            // First stop (conditional)
            When(x => !string.IsNullOrEmpty(x.FirstStopAddress), () =>
            {
                RuleFor(x => x.FirstStopAddress)
                    .MaximumLength(300).WithMessage("FirstStopAddress får inte överstiga 300 tecken.");

                RuleFor(x => x.FirstStopLatitude)
                    .NotNull().WithMessage("FirstStopLatitude krävs när FirstStopAddress anges.")
                    .InclusiveBetween(-90, 90).WithMessage("FirstStopLatitude måste vara mellan -90 och 90.");

                RuleFor(x => x.FirstStopLongitude)
                    .NotNull().WithMessage("FirstStopLongitude krävs när FirstStopAddress anges.")
                    .InclusiveBetween(-180, 180).WithMessage("FirstStopLongitude måste vara mellan -180 och 180.");
            });

            // Second stop (conditional)
            When(x => !string.IsNullOrEmpty(x.SecondStopAddress), () =>
            {
                RuleFor(x => x.SecondStopAddress)
                    .MaximumLength(300).WithMessage("SecondStopAddress får inte överstiga 300 tecken.");

                RuleFor(x => x.SecondStopLatitude)
                    .NotNull().WithMessage("SecondStopLatitude krävs när SecondStopAddress anges.")
                    .InclusiveBetween(-90, 90).WithMessage("SecondStopLatitude måste vara mellan -90 och 90.");

                RuleFor(x => x.SecondStopLongitude)
                    .NotNull().WithMessage("SecondStopLongitude krävs när SecondStopAddress anges.")
                    .InclusiveBetween(-180, 180).WithMessage("SecondStopLongitude måste vara mellan -180 och 180.");
            });

            // Dropoff
            RuleFor(x => x.DropOffAddress)
                .NotEmpty().WithMessage("DropOffAddress är obligatoriskt.")
                .MaximumLength(300).WithMessage("DropOffAddress får inte överstiga 300 tecken.");

            RuleFor(x => x.DropOffLatitude)
                .InclusiveBetween(-90, 90).WithMessage("DropOffLatitude måste vara mellan -90 och 90.");

            RuleFor(x => x.DropOffLongitude)
                .InclusiveBetween(-180, 180).WithMessage("DropOffLongitude måste vara mellan -180 och 180.");

            // Optional
            When(x => !string.IsNullOrEmpty(x.Flightnumber), () =>
            {
                RuleFor(x => x.Flightnumber)
                    .MaximumLength(20).WithMessage("Flightnumber får inte överstiga 20 tecken.");
            });
        }
    }
}
