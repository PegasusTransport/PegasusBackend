using PegasusBackend.Models.Roles;

namespace PegasusBackend.DTOs.UserDTOs
{
    public class UserResponseDto
    {
        public string UserName { get; set; } = string.Empty;    
        public string FirstName { get; set; } = string.Empty;   
        public string LastName { get; set; } = string.Empty;   
        public string Email{ get; set; } = string.Empty;   
    }
}
