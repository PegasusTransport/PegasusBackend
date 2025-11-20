using FluentValidation;
using PegasusBackend.DTOs.UserDTOs;

namespace PegasusBackend.Validators
{
    public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestDtoValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.UserName), () =>
            {
                RuleFor(x => x.UserName)
                    .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
                    .MaximumLength(50).WithMessage("Username can't exceed 50 characters.")
                    .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores and hyphens.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
            {
                RuleFor(x => x.FirstName)
                    .MaximumLength(50).WithMessage("First name can't exceed 50 characters.")
                    .Matches(@"^[a-zA-ZåäöÅÄÖ\s-]+$").WithMessage("First name can only contain letters, spaces and hyphens.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
            {
                RuleFor(x => x.LastName)
                    .MaximumLength(50).WithMessage("Last name can't exceed 50 characters.")
                    .Matches(@"^[a-zA-ZåäöÅÄÖ\s-]+$").WithMessage("Last name can only contain letters, spaces and hyphens.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Invalid phone number format.")
                    .MinimumLength(10).WithMessage("Phone number must be at least 10 characters.")
                    .MaximumLength(20).WithMessage("Phone number can't exceed 20 characters.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Invalid email format.")
                    .MaximumLength(100).WithMessage("Email can't exceed 100 characters.");
            });
        }
    }
}