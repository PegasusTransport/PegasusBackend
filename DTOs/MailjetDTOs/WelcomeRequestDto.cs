using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class WelcomeRequestDto
    {
        [Required(ErrorMessage = "Name is Required for the email")]
        [MaxLength(50, ErrorMessage = "The name can not be longer then 50 letters!")]
        public string firstname { get; set; } = string.Empty;
    }
}
