using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMailjetController : ControllerBase
    {
        [HttpGet("test")]
        public async Task<ActionResult> TestMailjetRespons([FromServices] IMailjetEmailService mailjet)
        {
            var result = await mailjet.SendEmailAsync(
                "Parman7000@yahoo.com",
                MailjetTemplateType.Welcome,
                new WelcomeDto
                {
                    firstname = "Parman"
                }
            );
            return Ok(result);
        }

    }
}
