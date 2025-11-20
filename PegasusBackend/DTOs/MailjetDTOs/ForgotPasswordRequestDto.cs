using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class ForgotPasswordRequestDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string ResetLink { get; set; } = string.Empty;
    }
}
