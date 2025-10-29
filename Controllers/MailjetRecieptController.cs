using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailjetRecieptController : ControllerBase
    {
        private readonly IMailjetEmailService _mailjet;

        public MailjetRecieptController(IMailjetEmailService mailjet)
        {
            _mailjet = mailjet;
        }

        [HttpPost("CreateReciept")]
        public async Task<ActionResult<bool>> CreateReciept([FromBody] ReceiptRequestDto dto) =>
            Generate.ActionResult(await _mailjet.MapRecieptAttachmentForMailjet(dto));

    }
}
