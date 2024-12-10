using FoosballProLeague.Webserver.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace FoosballProLeague.Webserver.Service
{
    public class AccountService : IAccountService
    {

        private readonly IHttpClientService _httpClientService;

        public AccountService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public async Task<HttpResponseMessage> FindUserByEmail(string email)
        {
            return await _httpClientService.GetAsync("api/User/user");
        }

        public async Task<HttpResponseMessage> ResetPassword(string email, string password)
        {
            object updateData = new { Email = email, Password = password };
            StringContent content = new StringContent(JsonConvert.SerializeObject(updateData), Encoding.UTF8, "application/json");

            return await _httpClientService.PutAsync("api/User/reset-password", content);
        }

        public async Task<HttpResponseMessage> SendEmail(string email)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");

            return await _httpClientService.PostAsync("api/User/send-email", content);
        }
    }
}
