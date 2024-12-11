using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;


namespace FoosballProLeague.Webserver.Service;

public class LoginService : ILoginService
{
    private readonly IHttpClientService _httpClientService;

    public LoginService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    //Service call for login user by calling the HttpClientService
    //Puts the login data in a object and serializes it
    public async Task<HttpResponseMessage> LoginUser(string email, string password)
    {
        object loginData = new { Email = email, Password = password };
        StringContent content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

        return await _httpClientService.PostAsync("api/user/login", content);
    }
}

