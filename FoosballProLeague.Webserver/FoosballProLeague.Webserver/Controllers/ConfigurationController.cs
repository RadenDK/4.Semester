using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Webserver.Controllers
{
    [ApiController]
    [Route("config")]
    public class ConfigurationController : Controller
    {
        private readonly IConfiguration _configuration;

        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("url")]
        public IActionResult GetApiUrl()
        {
            var apiUrl = _configuration["HttpClientSettings:BaseAddress"];
            return Ok(new { apiUrl });
        }
    }
}
