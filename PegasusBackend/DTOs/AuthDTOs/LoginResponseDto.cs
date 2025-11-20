using TypeGen.Core.TypeAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    [ExportTsInterface]
    public class LoginResponseDto
    {
        public string Email { get; set; } = null!;
    }
}
