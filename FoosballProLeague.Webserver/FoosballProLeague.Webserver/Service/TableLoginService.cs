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
    public async Task<HttpResponseMessage> TableLoginUser(TableLoginModel tableLoginModel)
    {
        TableLoginModel data = new TableLoginModel{ Email = tableLoginModel.Email, TableId = tableLoginModel.TableId, Side = tableLoginModel.Side };
        StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        return await _httpClientService.PostAsync("api/match/LoginOnTable", content);
    }

    public async Task<HttpResponseMessage> TableClearTeam(){
        return await _httpClientService.GetAsync("api/match/ClearPendingTeamsCache");
    }
}

