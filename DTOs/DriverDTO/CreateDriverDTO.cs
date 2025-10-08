using PegasusBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PegasusBackend.DTOs.DriverDTO
{
    public class CreateDriverDTO
    {
        [Required]
        public string ProfilePicture { get; set; } = string.Empty;
        public int CarId { get; set; }
    }
}