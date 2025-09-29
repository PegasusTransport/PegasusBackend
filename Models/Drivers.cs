using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Drivers
    {
        [Key]
        public Guid DriverId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; } = null!;

        [Required]
        public int CarId { get; set; } 
        [ForeignKey(nameof(CarId))]
        public Cars Car { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string ProfilePicture { get; set; } = string.Empty;

        public virtual ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();

    }
}
