namespace PegasusBackend.DTOs.DriverDTO
{
    public class DriverResponseDto
    {
        public Guid DriverId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public int? CarId { get; set; }

        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public int? CarCapacity { get; set; }
        public string? CarType { get; set; }
        public string? CarLicensePlate { get; set; }

    }
}
