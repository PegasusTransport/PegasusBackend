using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.Models
{
    public class Bookings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }

        // Nullable for guest bookings
        public string? UserIdFk { get; set; }

        // Guest information (used when UserIdFk is null)
        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        [MaxLength(50)]
        public string? GuestFirstName { get; set; }

        [MaxLength(50)]
        public string? GuestLastName { get; set; }

        [MaxLength(20)]
        public string? GuestPhoneNumber { get; set; }

        public Guid? DriverIdFK { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime BookingDateTime { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PickUpDateTime { get; set; }

        // Pickup location
        [Required]
        [MaxLength(300)]
        public string PickUpAdress { get; set; } = null!;

        [Required]
        public double PickUpLatitude { get; set; }

        [Required]
        public double PickUpLongitude { get; set; }

        // First stop (optional)
        [MaxLength(300)]
        public string? FirstStopAddress { get; set; }

        public double? FirstStopLatitude { get; set; }

        public double? FirstStopLongitude { get; set; }

        // Second stop (optional)
        [MaxLength(300)]
        public string? SecondStopAddress { get; set; }

        public double? SecondStopLatitude { get; set; }

        public double? SecondStopLongitude { get; set; }

        // Dropoff location
        [Required]
        [MaxLength(300)]
        public string DropOffAdress { get; set; } = null!;

        [Required]
        public double DropOffLatitude { get; set; }

        [Required]
        public double DropOffLongitude { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceKm { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DurationMinutes { get; set; }

        [MaxLength(20)]
        public string? Flightnumber { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        // Email confirmation
        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.PendingEmailConfirmation;

        [MaxLength(255)]
        public string? ConfirmationToken { get; set; } = null!;

        public DateTime? ConfirmationTokenExpiresAt { get; set; }

        public bool IsConfirmed { get; set; } = false;
        public bool IsAvailable { get; set; } = true;

        [ForeignKey(nameof(UserIdFk))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(DriverIdFK))]
        public virtual Drivers? Driver { get; set; }
    }

    public enum BookingStatus
    {
        PendingEmailConfirmation,
        Confirmed,
        Cancelled,
        Completed
    }

    public enum BookingStatusForCustomers
    {
        Confirmed,
        Cancelled,
        Completed
    }

}
