using FluentValidation;
using PegasusBackend.DTOs.UserDTOs;
using PegasusBackend.Models.Roles;

namespace PegasusBackend.Validators
{
    public class RegistrationRequestDtoValidator : AbstractValidator<RegistrationRequestDto>
    {
        public RegistrationRequestDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
                .MaximumLength(50).WithMessage("Username can't exceed 50 characters.")
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores and hyphens.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name can't exceed 50 characters.")
                .Matches(@"^[a-zA-ZåäöÅÄÖ\s-]+$").WithMessage("First name can only contain letters, spaces and hyphens.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name can't exceed 50 characters.")
                .Matches(@"^[a-zA-ZåäöÅÄÖ\s-]+$").WithMessage("Last name can only contain letters, spaces and hyphens.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(100).WithMessage("Email can't exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Invalid phone number format.")
                .MinimumLength(10).WithMessage("Phone number must be at least 10 characters.")
                .MaximumLength(20).WithMessage("Phone number can't exceed 20 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role specified.")
                .NotEqual(UserRoles.Admin).WithMessage("Cannot register as Admin through this endpoint.");
        }
    }
}