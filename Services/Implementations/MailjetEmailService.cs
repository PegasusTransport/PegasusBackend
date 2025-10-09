using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PegasusBackend.Configurations;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.Net;

namespace PegasusBackend.Services.Implementations
{
    public class MailjetEmailService : IMailjetEmailService
    {
        private readonly MailJetSettings _settings;
        private readonly MailjetClient _client;

        public MailjetEmailService(IOptions<MailJetSettings> options)
        {
            _settings = options.Value;
            _client = new MailjetClient(_settings.ApiKey, _settings.ApiSecret);
        }

        private MailjetRequest BuildMailjetRequest(string toEmail, long templateId, object variables)
        {
            return new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.Messages, new JArray 
            {
                new JObject 
                {
                    {"From", new JObject {
                        {"Email", _settings.FromEmail},
                        {"Name", _settings.FromName}
                    }},

                    {"To", new JArray 
                        {
                            new JObject 
                            {
                                {"Email", toEmail}
                            }
                        }
                    },
                    {"TemplateID", templateId},
                    {"TemplateLanguage", true},
                    {"Variables", JObject.FromObject(variables)}
                }   
            });
        }

        private long GetTemplateId(MailjetTemplateType type) => type switch
        {
            MailjetTemplateType.Welcome => _settings.mailjetTemplates.Welcome,
            MailjetTemplateType.ForgotPassword => _settings.mailjetTemplates.ForgotPassword,
            MailjetTemplateType.TwoFA => _settings.mailjetTemplates.TwoFA,
            MailjetTemplateType.PendingConfirmation => _settings.mailjetTemplates.PendingConfirmation,
            MailjetTemplateType.BookingConfirmation => _settings.mailjetTemplates.BookingConfirmation,
            MailjetTemplateType.Receipt => _settings.mailjetTemplates.Receipt,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown template type")
        };



        public async Task<ServiceResponse<bool>> SendEmailAsync(
        string toEmail,
        MailjetTemplateType templateType,
        object variables) // DTO specific for every template
        {
            try
            {
                var templateId = GetTemplateId(templateType);
                var request = BuildMailjetRequest(toEmail, templateId, variables);

                var response = await _client.PostAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return ServiceResponse<bool>.SuccessResponse(
                        HttpStatusCode.OK,
                        true,
                        $"Email ({templateType}) sent successfully to {toEmail}."
                    );
                }

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.BadRequest,
                    $"Mailjet returned an error: {templateType}"
                    );
            }
            catch (ArgumentOutOfRangeException)
            {
                return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.BadRequest,
                $"Unknown template type: {templateType}"
                );
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.FailResponse(
                HttpStatusCode.BadRequest,
                $"Unexpextead error sending {templateType}, email {ex.Message}"
                );
            }
        }

    }
}
