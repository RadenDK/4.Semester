using Microsoft.AspNetCore.Mvc.Filters; // For filters to intercept requests
using Microsoft.AspNetCore.Mvc;          // For result types, like UnauthorizedResult
using Microsoft.Extensions.Configuration; // To access configuration settings

namespace FoosballProLeague.Api.Controllers
{
    // Custom attribute to require an API key for certain actions or controllers
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        // Define the name of the header key where the API key should be passed
        private const string ApiKeyHeaderName = "X-Api-Key";

        // This method is called automatically before an action executes
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the request contains the API key in the headers
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                // If the header is missing, respond with "Unauthorized" (HTTP 401) and a custom message
                context.Result = new JsonResult(new { message = "API Key is missing" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return; // Stop further execution
            }

            // Get the API key from appsettings.json through IConfiguration
            // IConfiguration is injected to access the configuration file where sensitive information is stored
            IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            string apiKey = configuration.GetValue<string>("ApiKey");

            // Compare the extracted key from the request with the expected key from configuration
            if (!apiKey.Equals(extractedApiKey))
            {
                // If the keys do not match, respond with "Unauthorized" (HTTP 401) and a custom message
                context.Result = new JsonResult(new { message = "Invalid API Key" })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return; // Stop further execution
            }

            // If the API key is correct, continue with the request
            await next(); // Proceed to the action method that was originally requested
        }
    }
}
