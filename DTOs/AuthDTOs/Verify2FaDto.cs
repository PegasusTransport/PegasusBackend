using TypeGen.Core.TypeAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    [ExportTsInterface]
    public class Verify2FaDto
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
