using MimeKit;

namespace PegasusBackend.DTOs.EmailDTO
{
    public class Message(IEnumerable<(string name, string email)> recipients, string content, string subject)
    {
        public List<MailboxAddress> Recipients = [.. recipients.Select(r => new MailboxAddress(r.name, r.email))];
        public string Subject { get; set; } = subject;
        public string Content { get; set; } = content;

    }
}
