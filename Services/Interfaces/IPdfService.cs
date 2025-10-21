using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IPdfService
    {
        Task<ServiceResponse<byte[]>> GenerateReceiptPdfAsync(ReceiptRequestDto receiptData);
    }
}
