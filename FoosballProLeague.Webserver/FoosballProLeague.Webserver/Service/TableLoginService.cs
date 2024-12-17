using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;
using System.Collections.Generic;


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
        TableLoginModel data = new TableLoginModel { Email = tableLoginModel.Email, TableId = tableLoginModel.TableId, Side = tableLoginModel.Side };
        StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        return await _httpClientService.PostAsync("api/match/LoginOnTable", content);
    }

    public async Task<HttpResponseMessage> RemoveUser(int userId, int tableId)
    {
        HttpResponseMessage response = await _httpClientService.PostAsync($"api/match/RemovePendingUser?userId={userId}&tableId={tableId}", null);
        return response;
    }


    public async Task<Dictionary<string, List<UserModel>>> GetAllCurrentPendingUsers(int tableId)
    {
        // Fetch data from the API
        HttpResponseMessage response = await _httpClientService.GetAsync($"api/Match/PendingUsers/?tableid={tableId}");

        // Ensure the response was successful
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }

        // Deserialize and return the result
        string jsonString = await response.Content.ReadAsStringAsync();

        Dictionary<string, List<UserModel>> pendingUsersBySide = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<UserModel>>>(
            jsonString,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return pendingUsersBySide;
    }
}

