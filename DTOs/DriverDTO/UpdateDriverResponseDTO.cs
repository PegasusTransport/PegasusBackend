namespace PegasusBackend.DTOs.DriverDTO
{
    public class UpdateDriverResponseDTO
    {
        public Guid DriverId { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
        public int? CarId { get; set; }
    }
}