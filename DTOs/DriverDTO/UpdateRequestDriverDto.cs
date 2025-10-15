namespace PegasusBackend.DTOs.DriverDTO
{
    public class UpdateRequestDriverDto
    {
        public string? ProfilePicture { get; set; } = string.Empty;
        public int? CarId { get; set; }
    }
}