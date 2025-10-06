using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    public class LoginRequestDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
