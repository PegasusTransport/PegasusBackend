namespace PegasusBackend.DTOs.ChatbotDTOs
{
    public class ChatSession
    {
        public List<ChatMessageDto> Messages { get; set; } = [];
        public DateTime LastActivity { get; set; }
    }
}
