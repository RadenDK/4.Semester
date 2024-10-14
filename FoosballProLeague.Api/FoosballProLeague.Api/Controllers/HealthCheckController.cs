using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class HealthCheckController : ControllerBase
    {

        private static int count = 0;
        public HealthCheckController()
        {
        }


        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok("API is running");
        }

        [HttpGet("counter")]
        public IActionResult GetCounter()
        {
            count++;

            return Ok("Current call count since live time = " + count);
        }
    }
}
