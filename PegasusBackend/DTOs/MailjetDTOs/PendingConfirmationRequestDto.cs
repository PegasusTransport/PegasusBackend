using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class PendingConfirmationRequestDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string Stops { get; set; } = "No Stopps!";
        public string Destination { get; set; } = string.Empty;
        public string PickupTime { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}
