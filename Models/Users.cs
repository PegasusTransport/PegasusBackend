using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.Models
{
    public enum UserRole
    {
        Customer,
        Driver,
        Admin
    }
    public class Users
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(257)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public string? PasswordHash { get; set; }

        public virtual Drivers? Driver { get; set; }

        public virtual Customers? Customer { get; set; }
    }
}
