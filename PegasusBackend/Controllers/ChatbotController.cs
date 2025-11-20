using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Helpers;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController(IChatbotService chatbotService) : ControllerBase
    {
        [HttpPost("Chatbot")]
        public async Task<ActionResult<bool>> TalkToChatbot(string input) =>
            Generate.ActionResult(await chatbotService.GetAiResponse(input));
    }
}
