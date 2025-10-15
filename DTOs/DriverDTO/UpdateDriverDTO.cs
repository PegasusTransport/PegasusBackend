namespace PegasusBackend.DTOs.DriverDTO
{
    public class UpdateDriverDto
    {
        public string? ProfilePicture { get; set; } = string.Empty;
        public int? CarId { get; set; }
    }
}