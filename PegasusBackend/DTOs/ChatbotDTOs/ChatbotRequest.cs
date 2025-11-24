namespace PegasusBackend.DTOs.ChatbotDTOs
{
    public class ChatbotRequest
    {
        public string Input { get; set; } = null!;
        public Guid SessionId { get; set; } 
    }
}
