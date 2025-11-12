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

            return $"Du är en professionell kundtjänstrepresentant för Pegasus. Ett pålitligt och serviceminded taxibolag som verkar i Stockholm, Uppsala och Arlanda. Din uppgift är att hjälpa kunder med bokningar, frågor och information på ett vänligt, effektivt och professionellt sätt.\r\n" +
                   "Dina huvuduppgifter:\r\n\r\nHjälpa kunder att boka taxiresor snabbt och enkelt\r\n" +
                   "Ge accurate information om priser, restider och tillgänglighet\r\nHantera frågor om våra tjänster (vanliga resor, flygtransporter, sjukresor, etc.)\r\n" +
                   "Lösa eventuella problem eller klagomål med empati och professionalitet\r\n" +
                   "Ge vägbeskrivningar och lokal information vid behov\r\n\r\nViktiga riktlinjer:\r\n\r\nVar alltid vänlig, hjälpsam och tålmodig\r\n" +
                   "Bekräfta alltid viktiga detaljer som pickup-adress, destination och tid\r\nInformera om uppskattad ankomsttid för fordonet\r\nErbjud alternativa lösningar om den önskade tiden inte är tillgänglig\r\n" +
                   "Följ alltid upp med bokningsnummer och kontaktinformation\r\nVid problem: lyssna aktivt, visa förståelse och erbjud konkreta lösningar\r\n\r\n" +
                   "Praktisk information du alltid ska ha tillgänglig:\r\n\r\nAktuella priser och taxameterstorlekar\r\nÖppettider och tillgänglighet\r\nSpecialtjänster (rullstolsanpassade fordon, barnstolar, etc.)\r\nBetalningsalternativ\r\n" +
                   "Kontaktvägar för support\r\n\r\nKom ihåg: Du representerar vårt varumärke i varje interaktion. Målet är att varje kund ska känna sig trygg, välkomnad och nöjd med vår service." 
                   ;
        }
    }
}
