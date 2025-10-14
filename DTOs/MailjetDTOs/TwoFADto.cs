using System.ComponentModel.DataAnnotations;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class TwoFADto
    {
        [Required]
        [MaxLength(50)]
        public string Firstname { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits.")]
        public string VerificationCode { get; set; } = string.Empty;
    }
}
