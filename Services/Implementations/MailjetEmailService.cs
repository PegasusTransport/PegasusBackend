using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PegasusBackend.Configurations;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class MailjetEmailService : IMailjetEmailService
    {
        private readonly MailJetSettings _settings;
        private readonly MailjetClient _client;
        private readonly ILogger<MailjetEmailService> _logger;

        public MailjetEmailService(IOptions<MailJetSettings> options, ILogger<MailjetEmailService> logger)
        {
            _settings = options.Value;
            _client = new MailjetClient(_settings.ApiKey, _settings.SecretKey);
            _logger = logger;
        }

        private MailjetRequest BuildMailjetRequest(string toEmail, long templateId, object variables, string subject)
        {
            _logger.LogInformation("Building Mailjet request. FromEmail: {Email}, FromName: {Name}",
                _settings.SenderEmail, _settings.SenderName);

            return new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.Messages, new JArray
            {
                new JObject
                {

                    {"FromEmail", _settings.SenderEmail},
                    {"FromName", _settings.SenderName},

                    {"Recipients", new JArray {
                        new JObject {
                            {"Email", toEmail}
                        }
                    }},

                    {"Mj-TemplateID", templateId},
                    {"Mj-TemplateLanguage", "true"},

                    {"Vars", JObject.FromObject(variables)},

                    {"Subject", subject}
                }
            });
        }

        


        private long GetTemplateId(MailjetTemplateType type) => type switch
        {
            MailjetTemplateType.Welcome => _settings.Templates.Welcome,
            MailjetTemplateType.ForgotPassword => _settings.Templates.ForgotPassword,
            MailjetTemplateType.TwoFA => _settings.Templates.TwoFA,
            MailjetTemplateType.PendingConfirmation => _settings.Templates.PendingConfirmation,
            MailjetTemplateType.BookingConfirmation => _settings.Templates.BookingConfirmation,
            MailjetTemplateType.Receipt => _settings.Templates.Receipt,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown template type")
        };

        public async Task<ServiceResponse<bool>> SendEmailAsync(
            string toEmail,
            MailjetTemplateType templateType,
            object variables,
            string subject)
        {
            try
            {
                var templateId = GetTemplateId(templateType);
                var request = BuildMailjetRequest(toEmail, templateId, variables, subject);
                var response = await _client.PostAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = response.GetErrorMessage();
                    var responseData = response.GetData();

                    _logger.LogError("[MAILJET ERROR] Status: {StatusCode}", response.StatusCode);
                    _logger.LogError("[MAILJET ERROR] Details: {Details}", errorDetails ?? "No details");
                    _logger.LogError("[MAILJET ERROR] Data: {Data}", responseData);

                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest,
                        $"Mailjet returned error. Details: {errorDetails ?? "No details"} | Data: {responseData}"
                    );
                }

                _logger.LogInformation("Email sent successfully to {Recipient} using template {TemplateType}.", toEmail, templateType);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    $"Email sent successfully using template: {templateType}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email ({TemplateType}) to {Recipient}", templateType, toEmail);

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.BadRequest,
                    $"Unexpected error sending {templateType}, email {ex.Message}"
                );
            }
        }
    }
}
