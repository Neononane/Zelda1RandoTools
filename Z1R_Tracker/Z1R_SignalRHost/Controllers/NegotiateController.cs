using Microsoft.AspNetCore.Mvc;
using Z1RSignalRHost;

namespace Z1R_SignalRHost.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NegotiateController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Dynamically return the Hub URL clients should connect to.
            var response = new
            {
                url = $"http://{Startup.PublicHost}:5000/hub", // or https:// if behind reverse proxy
                accessToken = "" // Optional: leave empty to match expected schema
            };

            return Ok(response);
        }
    }
}
