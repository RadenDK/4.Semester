using Newtonsoft.Json;
using System.Text;

namespace FoosballProLeague.Webserver.Service;

public class TokenService : ITokenService
{

    private readonly IHttpClientService _httpClientService;

    public TokenService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }

    public async Task<HttpResponseMessage> ValidateJwtAndGetNewJwt(string jwt)
    {
        _httpClientService.SetAuthorizationHeader(jwt);
        return await _httpClientService.GetAsync("api/user/token/validate");
    }
}