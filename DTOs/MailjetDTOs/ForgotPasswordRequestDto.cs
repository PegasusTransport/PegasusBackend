using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Name is Required for the email")]
        [MaxLength(50, ErrorMessage = "The name can not be longer then 50 letters!")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Link for resetting the password is required.")]
        [Url(ErrorMessage = "Please send in a valid link.")] // Needs to validate that the link have https//: 
        public string ResetLink { get; set; } = string.Empty;
    }
}
