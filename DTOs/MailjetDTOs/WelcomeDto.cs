using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class WelcomeDto
    {
        [Required]
        public string firstname { get; set; } = string.Empty;
    }
}
