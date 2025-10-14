using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class BookingConfirmationDto
    {
        [Required]
        [MaxLength(50)]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PickupAddress { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Stops { get; set; } = "-";

        [Required]
        [MaxLength(200)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public string PickupTime { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal TotalPrice { get; set; }
    }
}
