namespace PegasusBackend.DTOs.MapDTOs
{
    public class AutoCompleteResponseDto
    {
        public List<string?>? Suggestions { get; set; } 
        public string SessionToken { get; set; } = null!;
    } 
}