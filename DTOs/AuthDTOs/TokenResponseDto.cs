using PegasusBackend.Models.Roles;

namespace PegasusBackend.DTOs.AuthDTOs
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool IsAuthenticated { get; set; }
        public IList<UserRoles> Roles { get; set; } = [];
    }
}
