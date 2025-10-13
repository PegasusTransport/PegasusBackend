namespace PegasusBackend.DTOs.DriverDTO
{
    public class DriverDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public int? CarId { get; set; }

    }
}
