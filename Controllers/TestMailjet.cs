using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMailjet : ControllerBase
    {
        [HttpGet("test")]
        public async Task<ActionResult> TestMailjetRespons([FromServices] IMailjetEmailService mailjet)
        {
            var result = await mailjet.SendEmailAsync(
                "Parman.gitijah@yahoo.com",
                MailjetTemplateType.Welcome,
                new { Name = "TestUser" }
            );
            return Ok(result);
        }

    }
}
