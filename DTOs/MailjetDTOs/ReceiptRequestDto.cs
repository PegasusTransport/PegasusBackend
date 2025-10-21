using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class ReceiptRequestDto
    {
        public int BookingId { get; set; } // Comes from vue
        public string CustomerFirstname { get; set; } = string.Empty; // Comes from vue
        public string PickupAddress { get; set; } = string.Empty; // Comes from vue
        public string? Stops { get; set; } = "Inga stopp angivna!"; // Comes from vue
        public string Destination { get; set; } = string.Empty; // Comes from vue
        public string DriverFirstname { get; set; } = string.Empty; // Comes from vue
        public string LicensePlate { get; set; } // Comes from vue
        public string? DriverImageUrl { get; set; } // Comes from vue

        public DateTime PickupTime { get; set; } // Comes from Driver
        public decimal DistanceKm { get; set; } // Comes from Driver
        public string DurationMinutes { get; set; } = string.Empty; // Comes from Driver
        public decimal TotalPrice { get; set; } // Comes from Driver
    }
}
