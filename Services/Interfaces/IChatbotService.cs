
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IChatbotService
    {
        Task<ServiceResponse<bool>> GetAiResponse(string input);
    }
}
