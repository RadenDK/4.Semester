using System.Net;
using System.Net.Http;
using System.Text;
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
   public async Task<HttpResponseMessage> LoginUser(LoginUserModel loginModel)
   {
       LoginUserModel loginUser = new LoginUserModel
       {
           Email = loginModel.Email,
           Password = loginModel.Password
       };
       string json = JsonSerializer.Serialize(loginUser);
       StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
        
       return await _httpClientService.PutAsync("/User/login", data);
       
       
   }
   
   
}

