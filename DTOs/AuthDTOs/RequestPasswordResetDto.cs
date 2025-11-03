using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    public class RequestPasswordResetDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}