using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class BookingConfirmationRequestDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string? Stops { get; set; } = "No stopps!";
        public string Destination { get; set; } = string.Empty;
        public string PickupTime { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }
}
