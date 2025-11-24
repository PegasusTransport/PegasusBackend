using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.ChatbotDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController(IChatbotService chatbotService) : ControllerBase
    {
        [HttpPost("olchatbot")]
        public async Task<ActionResult<bool>> TalkToChatbot(string input) =>
            Generate.ActionResult(await chatbotService.GetAiResponse(input));

        [HttpPost("Chatbot")]
        public async Task<ActionResult<bool>> ChatWithHistory(
            [FromBody] ChatbotRequest request) =>
            Generate.ActionResult(await chatbotService.GetAiResponseWithHistory(request));
    }
}
