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
   public async Task<HttpResponseMessage> LoginUser(string email, string password)
   {
       StringContent content = new StringContent(JsonConvert.SerializeObject(new{Email = email, Password = password} ), Encoding.UTF8, "application/json");
       
       return await _httpClientService.PostAsync("/api/User/login", content);
   }
}

