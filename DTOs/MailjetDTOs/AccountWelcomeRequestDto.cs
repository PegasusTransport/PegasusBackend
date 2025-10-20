using System.ComponentModel.DataAnnotations;
using PegasusBackend.Helpers.MailjetHelpers;

namespace PegasusBackend.DTOs.MailjetDTOs
{
    public class AccountWelcomeRequestDto
    {
        public string Firstname { get; set; } = string.Empty;
        public string VerificationLink { get; set; } = string.Empty;
        public string ButtonName { get; set; } = string.Empty;
    }
}
