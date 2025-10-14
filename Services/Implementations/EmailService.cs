using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Models;
using PegasusBackend.Services.Interfaces;
using System.Security.Authentication;

namespace PegasusBackend.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailConfig emailConfig, IConfiguration configuration, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(Message message)
        {
            var email = CreateEmailMessage(message);
            await SendAsync(email);
        }

        public async Task SendGuestConfirmationEmailAsync(
            string email,
            string firstName,
            string confirmationToken,
            Bookings booking)
        {
            try
            {
                // TODO: Implement with Mailjet
                var baseUrl = _configuration["ApplicationUrl"] ?? "https://localhost:7161";
                var confirmationUrl = $"{baseUrl}/api/booking/confirm?token={confirmationToken}";

                var subject = "Bekräfta din bokning hos Pegasus Transport";
                var emailContent = $@"
Hej {firstName}!

Tack för din bokning hos Pegasus Transport.

Bokningsdetaljer:
- Upphämtning: {booking.PickUpAdress}
- Avlämning: {booking.DropOffAdress}
- Datum & tid: {booking.PickUpDateTime:yyyy-MM-dd HH:mm}
- Pris: {booking.Price:F2} SEK

Klicka på länken nedan för att bekräfta din bokning:
{confirmationUrl}

Länken är giltig i 24 timmar.

Om du vill se dina bokningar i framtiden kan du registrera ett konto på vår hemsida med denna e-postadress.

Med vänliga hälsningar,
Pegasus Transport
";

                var message = new Message(
                    new[] { ("", email) },
                    emailContent,
                    subject
                );

                await SendEmailAsync(message);
                _logger.LogInformation("Guest confirmation email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending guest confirmation email to: {Email}", email);
                throw;
            }
        }

        public async Task SendRegisteredUserBookingEmailAsync(
            string email,
            string firstName,
            Bookings booking)
        {
            try
            {
                // TODO: Implement with Mailjet
                var subject = "Din bokning är bekräftad - Pegasus Transport";
                var emailContent = $@"
Hej {firstName}!

Din bokning hos Pegasus Transport är nu bekräftad!

Bokningsdetaljer:
- Bokningsnummer: #{booking.BookingId}
- Upphämtning: {booking.PickUpAdress}
- Avlämning: {booking.DropOffAdress}
- Datum & tid: {booking.PickUpDateTime:yyyy-MM-dd HH:mm}
- Pris: {booking.Price:F2} SEK

Du kan se alla dina bokningar genom att logga in på din profil på vår hemsida.

Vi ser fram emot att köra dig!

Med vänliga hälsningar,
Pegasus Transport
";

                var message = new Message(
                    new[] { ("", email) },
                    emailContent,
                    subject
                );

                await SendEmailAsync(message);
                _logger.LogInformation("Booking confirmation email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking confirmation email to: {Email}", email);
                throw;
            }
        }

        private async Task SendAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                client.AuthenticationMechanisms.Remove("XOAUTH2"); // To use App password in Gmail
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Recipients}",
                    string.Join(", ", message.To));
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP command error when sending email");
                throw;
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "SMTP protocol error when sending email");
                throw;
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Authentication error - check username/password");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when sending email");
                throw;
            }
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Pegasus Taxi", _emailConfig.From));
            emailMessage.To.AddRange(message.Recipients);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(TextFormat.Text) { Text = message.Content };

            return emailMessage;
        }
    }
}