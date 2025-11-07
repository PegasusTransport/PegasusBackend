using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    public class RequestPasswordResetDto
    {
        public string Email { get; set; } = string.Empty;
    }
}