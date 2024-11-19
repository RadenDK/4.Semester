using FoosballProLeague.Webserver.Models;
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


        public async Task<List<UserModel>> GetAllUsers()
        {
            HttpResponseMessage response = await _httpClientService.GetAsync("/api/User");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<UserModel>>(responseBody);
            }
            else
            {
                throw new Exception($"Could not get users. HTTP status code: {response.StatusCode}");
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
            else
            {
                throw new Exception($"Could not retrieve match history. HTTP status code: {response.StatusCode}");
            }
        }
    }
}
