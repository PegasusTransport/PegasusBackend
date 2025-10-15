using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.UserDTOs
{
    public class UpdateUserRequestDto
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; } 
        public string? LastName { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        [EmailAddress]
        public string? Email { get; set; } 
    }
}
