namespace FoosballProLeague.Webserver.Service;

public interface IHttpClientService
{
    Task<HttpResponseMessage> PostAsync(string url, StringContent content);

    Task<HttpResponseMessage> GetAsync(string url);
    
    Task<HttpResponseMessage> PutAsync(string url, StringContent content);
    
    void SetAuthorizationHeader(string accessToken);
}