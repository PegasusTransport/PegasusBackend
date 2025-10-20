using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IMailjetEmailService
    {
        Task<ServiceResponse<bool>> SendEmailAsync<T>(
            string toEmail,
            MailjetTemplateType templateType,
            T variables,
            string subject);
    }
}
