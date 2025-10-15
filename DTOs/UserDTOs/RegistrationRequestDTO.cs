using PegasusBackend.Models.Roles;
using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.UserDTOs
{
    public class RegistrationRequestDto
    {
        public string UserName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserRoles Role { get; set; } = UserRoles.User; // Default värde


    }
}
