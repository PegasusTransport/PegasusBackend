using FluentValidation;
using PegasusBackend.DTOs.BookingDTOs;

public class BookingFilterRequestDtoValidator : AbstractValidator<BookingFilterRequestForAdminDto>
{
    public BookingFilterRequestDtoValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue)
            .WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.HoursUntilPickup)
            .GreaterThanOrEqualTo(0)
            .When(x => x.HoursUntilPickup.HasValue)
            .WithMessage("HoursUntilPickup must be non-negative.");
    }
}
