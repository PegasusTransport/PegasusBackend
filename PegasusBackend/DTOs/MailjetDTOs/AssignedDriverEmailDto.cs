namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class AssignedDriverEmailDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string PickupAddress { get; set; } = string.Empty;
        public string Stops { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string PickupTime { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string DriverNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
    }

}
