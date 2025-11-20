namespace PegasusBackend.DTOs.DriverDTO
{
    public class AvailableDriverResponsDto
    {
        public Guid DriverId { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
    }
}
