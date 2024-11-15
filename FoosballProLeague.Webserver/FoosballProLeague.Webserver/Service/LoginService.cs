using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using FoosballProLeague.Webserver.Models;


namespace FoosballProLeague.Webserver.Service;

public class LoginService : ILoginService
{
   private readonly IHttpClientService _httpClientService;
   private readonly IHttpContextAccessor _httpContextAccessor;
   
   public LoginService(IHttpClientService httpClientService, IHttpContextAccessor httpContextAccessor)
   {
       _httpClientService = httpClientService;
       _httpContextAccessor = httpContextAccessor;
   }
   
   //Service call for login user by calling the HttpClientService
   public async Task<HttpResponseMessage> LoginUser(string email, string password)
   {
       StringContent content = new StringContent(JsonConvert.SerializeObject(new{Email = email, Password = password} ), Encoding.UTF8, "application/json");

       HttpResponseMessage response = await _httpClientService.PostAsync("/api/User/login", content);
       response.EnsureSuccessStatusCode();

       string responseBody = await response.Content.ReadAsStringAsync();
       UserModel user = JsonConvert.DeserializeObject<UserModel>(responseBody);

       CookieOptions cookieOptions = new CookieOptions
       {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
       };
       _httpContextAccessor.HttpContext.Response.Cookies.Append("User", JsonConvert.SerializeObject(user), cookieOptions);

       return response;
   }
}

