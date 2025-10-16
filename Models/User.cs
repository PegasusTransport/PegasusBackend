using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.Models
{
    public class User : IdentityUser
    {

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpireTime { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Drivers? Driver { get; set; }
    }

}
