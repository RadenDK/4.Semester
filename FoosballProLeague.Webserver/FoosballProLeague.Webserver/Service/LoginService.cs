using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using FoosballProLeague.Webserver.Models;


namespace FoosballProLeague.Webserver.Service;

public class LoginService : ILoginService
{
   private readonly IHttpClientService _httpClientService;
   
   public LoginService(IHttpClientService httpClientService)
   {
       _httpClientService = httpClientService;
   }
   
   //Service call for login user by calling the HttpClientService
   // Puts the login data in a object and serializes it
   public async Task<HttpResponseMessage> LoginUser(string email, string password)
   {
       object loginData = new { Email = email, Password = password };
       StringContent content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");
       
       // Set the authorization header with the accesstoken
       string accessToken = "Bearer " + "accesstoken";
       _httpClientService.SetAuthorizationHeader(accessToken);
       
       return await _httpClientService.PostAsync("User/login", content);
   }
}

