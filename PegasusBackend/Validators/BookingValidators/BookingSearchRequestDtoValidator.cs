using FluentValidation;
using PegasusBackend.DTOs.BookingDTOs;

public class BookingSearchRequestDtoValidator : AbstractValidator<BookingSearchRequestDto>
{
    public BookingSearchRequestDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .When(x => x.Page.HasValue)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .When(x => x.PageSize.HasValue)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
    }
}
