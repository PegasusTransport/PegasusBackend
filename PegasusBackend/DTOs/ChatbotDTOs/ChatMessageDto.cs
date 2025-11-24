namespace PegasusBackend.DTOs.ChatbotDTOs
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
