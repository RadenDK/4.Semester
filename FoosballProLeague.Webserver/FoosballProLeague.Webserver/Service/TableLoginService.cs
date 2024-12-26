using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;


namespace FoosballProLeague.Webserver.Service;

public class TableLoginService : ITableLoginService
{
    private readonly IHttpClientService _httpClientService;

    public TableLoginService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    //Service call for login user by calling the HttpClientService
    // Puts the login data in a object and serializes it
    public async Task<HttpResponseMessage> TableLoginUser(TableLoginViewModel tableLoginModel)
    {
        TableLoginViewModel data = new TableLoginViewModel { Email = tableLoginModel.Email, TableId = tableLoginModel.TableId, Side = tableLoginModel.Side };
        StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        return await _httpClientService.PostAsync("api/match/LoginOnTable", content);
    }

    public async Task<HttpResponseMessage> PendingUsers(int tableId)
    {
       return await _httpClientService.GetAsync($"api/match/{tableId}/PendingTeamUsers");

    }

    public async Task<HttpResponseMessage> RemoveUser(string email){

        StringContent content = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");

        return await _httpClientService.PostAsync("api/match/RemovePendingUser", content);
    }

    public async Task<HttpResponseMessage> ActiveMatch()
    {
        return await _httpClientService.GetAsync("api/match/Active");
    }
}

