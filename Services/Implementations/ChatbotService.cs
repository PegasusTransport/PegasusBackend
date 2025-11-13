using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System.ClientModel;
using System.Net;
using System.Threading.Tasks;

namespace PegasusBackend.Services.Implementations
{
    public class ChatbotService: IChatbotService
    {
        private readonly AzureOpenAIClient _azureClient;
        private readonly string? _deploymentName;
        private IAdminRepo _adminRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(IConfiguration configuration, IAdminRepo adminRepo, ILogger<ChatbotService> logger)
        {
            _logger = logger;
            _adminRepo = adminRepo;
            _configuration = configuration;
            _deploymentName = _configuration["ChatbotSettings:DeploymenName"];

            string? apiKey = _configuration["ChatbotSettings:Apikey"];
            string? endpoint = _configuration["ChatbotSettings:Endpoint"];

            _logger.LogInformation("Azure OpenAI API Key status: {ApiKeyStatus}",
       string.IsNullOrEmpty(apiKey) ? "Missing" : $"Present (length: {apiKey.Length})");

            _logger.LogInformation("Azure OpenAI Endpoint configured: {Endpoint}", endpoint);

            _logger.LogInformation("Azure OpenAI Deployment name: {DeploymentName}", _deploymentName);

            _azureClient = new AzureOpenAIClient(new Uri(endpoint!), new ApiKeyCredential(apiKey!));
        }

        public async Task<ServiceResponse<bool>> GetAiResponse(string input)
        {
            try
            {
                var chatClient = _azureClient.GetChatClient(_deploymentName);

                _logger.LogInformation($" USer: {input}");
                var messages = new ChatMessage[]
                {
                    new SystemChatMessage(await ContextForChatbotAsync()),
                    new UserChatMessage(input)
                };
               
                var chatCompletion = await chatClient.CompleteChatAsync(messages);
                string response = chatCompletion.Value.Content[0].Text;
                
                if (response == null)
                {
                    return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.BadRequest, "No response founf");
                }

                return ServiceResponse<bool>.SuccessResponse(HttpStatusCode.OK,
                    true,
                    response
                    );

            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");

                return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.InternalServerError, "Something went wrong");
            }

        }
        private async Task<string> ContextForChatbotAsync()
        {
            var prices = await _adminRepo.GetTaxiPricesAsync();


            if (prices == null)
            {
                _logger.LogInformation("Prices are null");
                return ContextText();
            }

            return $"{ContextText()} Pegasus prices: Start price:{100}, KM price:{prices.KmPrice}, Minute price:{prices.MinutePrice} Zone price: {prices.ZonePrice}";


        }
        private static string ContextText()
        {

            return "You are a professional customer service representative for Pegasus — a reliable and service-minded taxi company operating in Stockholm, Uppsala, and Arlanda. Your job is to assist customers with bookings, questions, and information in a friendly, efficient, and professional manner.\r\n\r\nAlways reply in the same language that the customer uses.\r\n\r\nYour main responsibilities:\r\n\r\n- Help customers book taxi rides quickly and easily  \r\n- Provide accurate information about prices, travel times, and availability  \r\n- Handle inquiries about our services (standard trips, airport transfers, medical transport, etc.)  \r\n- Resolve any issues or complaints with empathy and professionalism  \r\n- Provide directions and local information when needed  \r\n\r\nImportant guidelines:\r\n\r\n- Always be friendly, helpful, and patient  \r\n- Always confirm key details such as pickup address, destination, and time  \r\n- Inform customers about the estimated arrival time of the vehicle  \r\n- Offer alternative solutions if the requested time is not available  \r\n- Always follow up with a booking number and contact information  \r\n- In case of problems: listen actively, show understanding, and offer concrete solutions  \r\n\r\nEssential practical information to keep in mind:\r\n\r\n- Current pricing and fare rates  \r\n- Operating hours and availability  \r\n- Special services (wheelchair-accessible vehicles, child seats, etc.)  \r\n- Payment options  \r\n- Support contact channels  \r\n\r\nRemember: You represent the Pegasus brand in every interaction. The goal is for every customer to feel safe, welcomed, and satisfied with our service.\r\n";
                   
        }
    }
}
