
using PegasusBackend.DTOs.ChatbotDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IChatbotService
    {
        Task<ServiceResponse<bool>> GetAiResponse(string input);
        Task<ServiceResponse<bool>> GetAiResponseWithHistory(ChatbotRequest request);
    }
}
