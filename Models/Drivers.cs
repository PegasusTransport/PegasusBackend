using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Drivers
    {
        [Key]
        public Guid DriverId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        public Cars Car { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string ProfilePicture { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; } 

        public virtual ICollection<Bookings> Bookings { get; set; } = [];

    }
}
