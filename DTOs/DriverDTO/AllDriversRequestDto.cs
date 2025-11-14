using PegasusBackend.Models;

namespace PegasusBackend.DTOs.DriverDTO
{
    public class AllDriversRequestDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string RegNr { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
    }
}