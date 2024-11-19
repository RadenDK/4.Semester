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
            System.Console.WriteLine("API is running");
            return Ok("API is running");
        }

        [HttpGet("counter")]
        public IActionResult GetCounter()
        {
            count++;
            System.Console.WriteLine("Current call count since live time = " + count);
            return Ok("Current call count since live time = " + count);
        }

        // Modify the Test endpoint to accept a POST request with a JSON body
        [HttpPost("test")]
        public async Task<IActionResult> Test()
        {
            // Read the request body as a string
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                // Print the body to the console
                Console.WriteLine("Received JSON Body: " + body);
            }

            // Return a simple response
            return Ok("JSON body received");
        }
    }
}
