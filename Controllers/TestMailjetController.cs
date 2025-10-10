using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMailjetController : ControllerBase
    {
        private readonly IMailjetEmailService _mailjet;

        public TestMailjetController(IMailjetEmailService mailjet)
        {
            _mailjet = mailjet;
        }

        [HttpGet("testWelcomeMail")]
        public async Task<ActionResult<bool>> TestMailjetRespons(string yourEmail, string yourName) =>
            Generate.ActionResult(await _mailjet.SendEmailAsync(
                yourEmail,
                MailjetTemplateType.Welcome,
                new WelcomeDto { firstname = yourName },
                "🚖 Välkommen till Pegasus Transport 🚖"));


    }
}
