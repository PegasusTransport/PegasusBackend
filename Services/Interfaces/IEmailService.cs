using PegasusBackend.DTOs.EmailDTO;

namespace PegasusBackend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(Message message);
    }
}
