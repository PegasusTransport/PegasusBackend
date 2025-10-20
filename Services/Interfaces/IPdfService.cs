using PegasusBackend.DTOs.MailjetDTOs;

namespace PegasusBackend.Services.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateReceiptPdfAsync(BookingConfirmationRequestDto variabel);
    }
}
