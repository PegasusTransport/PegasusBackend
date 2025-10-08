using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class WelcomeDto
    {
        [Required]
        public string Name { get; set; }
    }
}
