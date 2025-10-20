using FluentValidation;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.MailjetDTOs;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IPdfService _Pdf;

        public MailjetEmailService(IOptions<MailJetSettings> options, ILogger<MailjetEmailService> logger, IServiceProvider serviceProvider, IPdfService pdf)
        {
            _settings = options.Value;
            _client = new MailjetClient(_settings.ApiKey, _settings.SecretKey);
            _logger = logger;
            _serviceProvider = serviceProvider;
            _Pdf = pdf;
        }

        private async Task<ServiceResponse<bool>?> ValidateEmailDtoAsync<T>(T dto, MailjetTemplateType templateType)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator == null)
            {
                _logger.LogDebug("No validator found for DTO type {DtoType}", typeof(T).Name);

                return null; // nothing to validate, keep going forvard!
            }

            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var errors = string.Join(", ", validation.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for {Template}: {Errors}", templateType, errors);

                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.BadRequest,
                    $"Validation failed for {templateType}: {errors}"
                );
            }

            return null;
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

        public async Task<ServiceResponse<bool>> SendEmailAsync<T>(
            string toEmail,
            MailjetTemplateType templateType,
            T variables,
            string subject)
        {
            try
            {
                var validationResult = await ValidateEmailDtoAsync(variables, templateType);
                if (validationResult != null)
                    return validationResult;

                var templateId = GetTemplateId(templateType);
                var request = BuildMailjetRequest(toEmail, templateId, variables!, subject);
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

        public async Task<ServiceResponse<bool>> SendEmailWithAttachmentAsync<T>(
            string toEmail,
            MailjetTemplateType templateType,
            T variables,
            string subject,
            byte[] pdfAttachment,
            string attachmentFileName = "receipt.pdf")
        {
            try
            {
                var validationResult = await ValidateEmailDtoAsync(variables, templateType);
                if (validationResult != null)
                    return validationResult;

                var templateId = GetTemplateId(templateType);

                var request = BuildMailjetRequestWithAttachment(
                    toEmail,
                    templateId,
                    variables!,
                    subject,
                    pdfAttachment,
                    attachmentFileName);

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

                _logger.LogInformation("Email with attachment sent successfully to {Recipient} using template {TemplateType}.", toEmail, templateType);
                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK,
                    true,
                    $"Email with attachment sent successfully using template: {templateType}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email with attachment ({TemplateType}) to {Recipient}", templateType, toEmail);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    $"Unexpected error sending {templateType} email with attachment: {ex.Message}"
                );
            }
        }



        public async Task<ServiceResponse<bool>> SendReceiptEmailAsync(
            string toEmail,
            BookingConfirmationRequestDto dto,
            string subject,
            string attachmentFileName = "receipt.pdf")
        {
            try
            {
                var pdfBytes = await _Pdf.GenerateReceiptPdfAsync(dto);

                return await SendEmailWithAttachmentAsync(
                    toEmail,
                    MailjetTemplateType.Receipt,
                    dto,
                    subject,
                    pdfBytes,
                    attachmentFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or sending PDF receipt to {Recipient}", toEmail);
                return ServiceResponse<bool>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    $"Error generating or sending PDF: {ex.Message}");
            }
        }

        private MailjetRequest BuildMailjetRequestWithAttachment(
            string toEmail,
            long templateId,
            object variables,
            string subject,
            byte[] attachmentData,
            string attachmentFileName)
        {
            var base64Attachment = Convert.ToBase64String(attachmentData);

            _logger.LogInformation("Building Mailjet v3 request with attachment. From: {Email}", _settings.SenderEmail);

            return new MailjetRequest
            {
                Resource = Send.Resource 
            }
            .Property(Send.FromEmail, _settings.SenderEmail)
            .Property(Send.FromName, _settings.SenderName)
            .Property(Send.Subject, subject)
            .Property(Send.MjTemplateID, templateId)
            .Property(Send.MjTemplateLanguage, true)
            .Property(Send.Vars, JObject.FromObject(variables))
            .Property(Send.Recipients, new JArray
            {
            new JObject { { "Email", toEmail } }
            })
            .Property(Send.Attachments, new JArray
            {
            new JObject
            {
                { "Content-type", "application/pdf" },
                { "Filename", attachmentFileName },
                { "content", base64Attachment } 
            }
            });
        }



    }
}
