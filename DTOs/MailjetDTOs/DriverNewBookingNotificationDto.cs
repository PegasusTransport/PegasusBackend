namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class DriverNewBookingNotificationDto
    {
        public string DriverFirstname { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string PickupTime { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string? Stops { get; set; }
        public decimal EstimatedPrice { get; set; }
        public decimal DistanceKm { get; set; }
        public string PortalLink { get; set; } = string.Empty;
    }
}