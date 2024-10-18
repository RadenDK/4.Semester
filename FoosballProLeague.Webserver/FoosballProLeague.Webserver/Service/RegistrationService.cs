using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;

public class RegistrationService : IRegistrationService
{
    private readonly IHttpClientService _httpClientService;
    
    public RegistrationService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        UserRegistrationModel user = new UserRegistrationModel
        {
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            Password = newUser.Password
        };
        
        string json = JsonSerializer.Serialize(user);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        return await _httpClientService.PostAsync("/User/user", content);
    }
}