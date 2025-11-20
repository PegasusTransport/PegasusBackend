namespace PegasusBackend.DTOs.AuthDTOs
{
    public class RefreshTokenRequestDto
    {
        public required string UserId { get; set; } = null!;
        public required string RefreshToken { get; set; } = null!;
    }
}
