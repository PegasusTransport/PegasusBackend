using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Hello from Pegasus 🚖");
        }
    }
}
