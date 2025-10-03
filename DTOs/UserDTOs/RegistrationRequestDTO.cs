using PegasusBackend.Models.Roles;
using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.UserDTOs
{
    public class RegistrationRequestDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = null!;
        [Required(ErrorMessage = "Last name is required.")] 
        public string LastName { get; set; } = null!;
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
        public UserRoles Role { get; set; } = UserRoles.User; // Default värde


    }
}
