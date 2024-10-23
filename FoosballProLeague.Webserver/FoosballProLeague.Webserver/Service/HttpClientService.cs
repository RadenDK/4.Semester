using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace FoosballProLeague.Webserver.Service;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private Uri BaseAddress = new Uri("http://localhost:5001/");

    public HttpClientService(HttpClient httpClient)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = BaseAddress
        };

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<HttpResponseMessage> PostAsync(string url, StringContent content)
    {
        return await _httpClient.PostAsync(url, content);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await _httpClient.GetAsync(url);
    }
    
    public async Task<HttpResponseMessage> PutAsync(string url, StringContent content)
    {
        return await _httpClient.PutAsync(url, content);
    }
}