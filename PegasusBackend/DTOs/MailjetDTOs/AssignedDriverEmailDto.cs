namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class AssignedDriverEmailDto
    {
        public string FirstName { get; set; }

        public string PickupAddress { get; set; }
        public string Stops { get; set; }             
        public string Destination { get; set; }
        public string PickupTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string DriverName { get; set; }
        public string DriverNumber { get; set; }
        public string LicensePlate { get; set; }
    }

}
