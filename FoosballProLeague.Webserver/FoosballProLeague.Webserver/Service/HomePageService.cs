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


        public async Task<List<UserModel>> GetUsers()
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
    }
}
