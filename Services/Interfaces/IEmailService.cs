using PegasusBackend.DTOs.EmailDTO;
using PegasusBackend.Models;

namespace PegasusBackend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(Message message);

        Task SendGuestConfirmationEmailAsync(
            string email,
            string firstName,
            string confirmationToken,
            Bookings booking);

        Task SendRegisteredUserBookingEmailAsync(
            string email,
            string firstName,
            Bookings booking);
    }
}