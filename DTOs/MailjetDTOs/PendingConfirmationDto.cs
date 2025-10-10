using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class PendingConfirmationDto
    {
        [Required(ErrorMessage = "Firstname is required.")]
        [MaxLength(50, ErrorMessage = "Firstname cannot exceed 50 characters.")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pickup address is required.")]
        [MaxLength(200, ErrorMessage = "Pickup address cannot exceed 200 characters.")]
        public string PickupAddress { get; set; } = string.Empty;


        [MaxLength(300, ErrorMessage = "Stops text cannot exceed 300 characters.")]
        public string Stops { get; set; } = "No stops specified";


        [Required(ErrorMessage = "Destination is required.")]
        [MaxLength(200, ErrorMessage = "Destination cannot exceed 200 characters.")]
        public string Destination { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pickup time is required.")]
        [MaxLength(100, ErrorMessage = "Pickup time format is too long.")]
        public string PickupTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total price is required.")]
        [Range(0, 10000, ErrorMessage = "Total price must be between 0 and 10000 SEK.")]
        public decimal TotalPrice { get; set; }


        [Required(ErrorMessage = "Confirmation link is required.")]
        [Url(ErrorMessage = "Invalid confirmation link URL format.")]
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}
