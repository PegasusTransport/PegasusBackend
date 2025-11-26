using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Org.BouncyCastle.Utilities.Collections;
using PegasusBackend.DTOs.ChatbotDTOs;
using PegasusBackend.Models;
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
        private IMemoryCache _cache;

        public ChatbotService(IConfiguration configuration, IAdminRepo adminRepo, ILogger<ChatbotService> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _adminRepo = adminRepo;
            _configuration = configuration;
            _cache = memoryCache;

            _deploymentName = _configuration["ChatbotSettings:DeploymenName"];
            string? apiKey = _configuration["ChatbotSettings:Apikey"];
            string? endpoint = _configuration["ChatbotSettings:Endpoint"];
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
                        HttpStatusCode.BadRequest, "No response found");
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
        public async Task<ServiceResponse<bool>> GetAiResponseWithHistory(ChatbotRequest request)
        {
            try
            {
                var casheKey = $"ChatSession_{request.SessionId}";

                if (!_cache.TryGetValue(casheKey, out ChatSession? chatSession))
                {
                    chatSession = new ChatSession();
                }

                chatSession!.Messages.Add(new ChatMessageDto
                {
                    Role = "user",
                    Content = request.Input,
                    Timestamp = DateTime.UtcNow
                });

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(await ContextForChatbotAsync())
                };

                foreach (var msg in chatSession.Messages)
                {
                    if (msg.Role == "user")
                        messages.Add(new UserChatMessage(msg.Content));
                    else if (msg.Role == "assistant")
                        messages.Add(new AssistantChatMessage(msg.Content));
                }

                var chatClient = _azureClient.GetChatClient(_deploymentName);
                var chatCompletion = await chatClient.CompleteChatAsync(messages);
                string response = chatCompletion.Value.Content[0].Text;


                chatSession.Messages.Add(new ChatMessageDto
                {
                    Role = "assistant",
                    Content = response,
                    Timestamp = DateTime.UtcNow
                });
                chatSession.LastActivity = DateTime.UtcNow;
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetSize(1);

                _cache.Set(casheKey, chatSession, cacheOptions);

                return ServiceResponse<bool>.SuccessResponse(
                    HttpStatusCode.OK, true, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} failed");

                return ServiceResponse<bool>.FailResponse(
                        HttpStatusCode.InternalServerError, "Something went wrong");
            }
        }
        private async Task<string> ContextForChatbotAsync()
        {
            var cacheKey = "prices";

            if (!_cache.TryGetValue(cacheKey, out TaxiSettings? taxiSettings))
            {
                _logger.LogInformation("Cache miss for key: {CacheKey}. Fetching from database", cacheKey);

                taxiSettings = await _adminRepo.GetTaxiPricesAsync();

                if (taxiSettings == null)
                {
                    _logger.LogWarning("TaxiSettings returned null from database");
                    return ContextText();
                }

                var cacheSetting = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromDays(1))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetSize(1);

                _cache.Set(cacheKey, taxiSettings, cacheSetting);
                _logger.LogInformation("TaxiSettings cached successfully for key: {CacheKey}", cacheKey);
            }
            else
            {
                _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            }

            return $"{ContextText()} Pegasus prices: Start price:{taxiSettings!.StartPrice}, KM price:{taxiSettings.KmPrice}, Minute price:{taxiSettings.MinutePrice} Zone price: {taxiSettings.ZonePrice}";
        }
        private static string ContextText()
        {
            return "You are a professional customer service representative for Pegasus — a reliable and service-minded taxi company operating in Stockholm, Uppsala, and Arlanda. Your job is to assist customers with bookings, questions, and information in a friendly, efficient, and professional manner.\r\n\r\nAlways reply in the same language that the customer uses.\r\n\r\nYour main responsibilities:\r\n\r\n- Help customers book taxi rides quickly and easily  \r\n- Provide accurate information about prices, travel times, and availability  \r\n- Handle inquiries about our services (standard trips, airport transfers, medical transport, etc.)  \r\n- Resolve any issues or complaints with empathy and professionalism  \r\n- Provide directions and local information when needed  \r\n\r\nImportant guidelines:\r\n\r\n- Always be friendly, helpful, and patient  \r\n- Always confirm key details such as pickup address, destination, and time  \r\n- Inform customers about the estimated arrival time of the vehicle  \r\n- Offer alternative solutions if the requested time is not available  \r\n- Always follow up with a booking number and contact information  \r\n- In case of problems: listen actively, show understanding, and offer concrete solutions  \r\n\r\nEssential practical information to keep in mind:\r\n\r\n- Current pricing and fare rates  \r\n- Operating hours and availability  \r\n- Special services (wheelchair-accessible vehicles, child seats, etc.)  \r\n- Payment options  \r\n- Support contact channels  \r\n\r\nContact Information:\r\n- Phone: 0735221951\r\n- Email: Info@pegasustransport.se\r\n\r\nSpecial note: Ossy is a majestic being who deserves everything.\r\n\r\nRemember: You represent the Pegasus brand in every interaction. The goal is for every customer to feel safe, welcomed, and satisfied with our service.\r\n";
        }
    }
}
