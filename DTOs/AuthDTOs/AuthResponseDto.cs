using PegasusBackend.Models.Roles;
using TypeGen.Core.TypeAnnotations;

namespace PegasusBackend.DTOs.AuthDTOs
{
    [ExportTsInterface]
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public IList<UserRoles> Roles { get; set; } = [];
        public int AccessTokenExpiresIn { get; set; }


    }
}
