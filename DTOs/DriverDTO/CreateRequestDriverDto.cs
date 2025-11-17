using PegasusBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.DTOs.DriverDTO
{
    public class CreateRequestDriverDto
    {
        public string Email { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
    }
}