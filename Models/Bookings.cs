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
        public string UserIdFk { get; set; } = null!;

        public Guid? DriverIdFK { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime BookingDateTime { get; set; }

        [Required]
        public DateTime PickUpDateTime { get; set; }

        //public string PickUpAdress { get; set; } Google maps api skickar med adress också. 

        [Required]
        public double PickUpLatitude { get; set; }

        [Required]
        public double PickUpLongitude { get; set; }

        //public string DropOffAdress { get; set; } Google maps api skickar med adress också. 

        [Required]
        public double DropOffLatitude { get; set; }

        [Required]
        public double DropOffLongitude { get; set; }

        [MaxLength(20)]
        public string? Flightnumber { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
        public bool IsConfirmed { get; set; } = false;
        public bool IsAvailable { get; set; } = true;

        [ForeignKey(nameof(UserIdFk))]
        [Required]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(DriverIdFK))]
        public virtual Drivers? Driver { get; set; }
    }
}
