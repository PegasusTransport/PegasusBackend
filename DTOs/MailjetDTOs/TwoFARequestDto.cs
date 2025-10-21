using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class TwoFARequestDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
