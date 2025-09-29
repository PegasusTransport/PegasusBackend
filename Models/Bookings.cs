using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Bookings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        [Required]
        public Guid CustomerIdFK { get; set; }

        public Guid? DriverIdFK { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime BookingDateTime { get; set; }

        [Required]
        public DateTime PickUpDateTime { get; set; }

        [Required]
        public double PickUpCoordinate { get; set; }

        [Required]
        public double DropOffCoordinate { get; set; }

        [MaxLength(20)]
        public string? Flightnumber { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public bool IsAvailable { get; set; } = true;

        [ForeignKey(nameof(CustomerIdFK))]
        [Required]
        public virtual Customers Customer { get; set; } = null!;

        [ForeignKey(nameof(DriverIdFK))]
        public virtual Drivers? Driver { get; set; }
    }
}
