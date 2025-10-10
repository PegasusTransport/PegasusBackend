using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class WelcomeDto
    {
        [Required(ErrorMessage = "Name is Required for the email")]
        [MaxLength(50, ErrorMessage = "The name can not be longer then 50 alphabets!")]
        public string firstname { get; set; } = string.Empty;
    }
}
