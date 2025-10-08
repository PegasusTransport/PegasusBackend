using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IMailjetEmailService
    {
        Task<ServiceResponse<bool>> SendEmailAsync(string toEmail, MailjetTemplateType templateType, object variables);
    }
}
