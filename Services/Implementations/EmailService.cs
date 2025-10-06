using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Services.Interfaces;
using System.Security.Authentication;

namespace PegasusBackend.Services.Implementations
{
    public class EmailService(EmailConfig emailConfig, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendEmailAsync(Message message)
        {
            // Create the email message
            var email = CreateEmailMessage(message);

            // Send the email
            await SendAsync(email);
        }
        private async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                client.AuthenticationMechanisms.Remove("XOAUTH2"); // TO use App password in gmail
                await client.ConnectAsync(emailConfig.SmtpServer, emailConfig.Port, true);
                await client.AuthenticateAsync(emailConfig.Username, emailConfig.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                logger.LogInformation("Email sent to {Recipients}",
                    string.Join(", ", message.To));
            }
            catch (SmtpCommandException ex)
            {
                logger.LogError(ex, "SMTP command error when sending email");
                throw;
            }
            catch (SmtpProtocolException ex)
            {
                logger.LogError(ex, "SMTP protocol error when sending email");
                throw;
            }
            catch (AuthenticationException ex)
            {
                logger.LogError(ex, "Authentication error - check username/password");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error when sending email");
                throw;
            }
        }
        private MimeMessage CreateEmailMessage(Message message)
        {
            // Create the email message
            var emailMessage = new MimeMessage();

            // Set the sender, recipients, subject, and body
            emailMessage.From.Add(new MailboxAddress("Pegasus Taxi", emailConfig.From));
            emailMessage.To.AddRange(message.Recipients);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }
    }
}
