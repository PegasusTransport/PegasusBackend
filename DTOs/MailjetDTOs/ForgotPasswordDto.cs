using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Name is Required for the email")]
        [MaxLength(50, ErrorMessage = "The name can not be longer then 50 alphabets!")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Link for reseting the password is requaired.")]
        [Url(ErrorMessage = "Please send in a valid link.")]
        public string ResetLink { get; set; } = string.Empty;
    }
}
