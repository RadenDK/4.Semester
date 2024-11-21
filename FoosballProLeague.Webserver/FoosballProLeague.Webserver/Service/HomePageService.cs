using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FoosballProLeague.Webserver.Service
{
    public class HomePageService : IHomePageService
    {
        private readonly IHttpClientService _httpClientService;

        public HomePageService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }


        public async Task<Dictionary<string, List<UserModel>>> GetLeaderboards()
        {
            HttpResponseMessage response = await _httpClientService.GetAsync("/api/User");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, List<UserModel>>>(responseBody);
            }
            else
            {
                throw new Exception($"Could not get users. HTTP status code: {response.StatusCode}");
            }
        }

        public async Task<MatchModel> GetActiveMatch()
        {
            HttpResponseMessage response = await _httpClientService.GetAsync("/GetActiveMatch");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseBody))
                {
                    return null;
                }
                return JsonSerializer.Deserialize<MatchModel>(responseBody);
            }
            else
            {
                throw new Exception($"Could not retrieve active match. HTTP status code: {response.StatusCode}");
            }
        }
        
        public async Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId)
        {
            HttpResponseMessage response = await _httpClientService.GetAsync($"/api/User/{userId}/match-history");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                
                return JsonSerializer.Deserialize<List<MatchHistoryModel>>(responseBody);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw new Exception($"Could not retrieve match history. HTTP status code: {response.StatusCode}");
            }
        }
    }
}
