using System.Net.Http;
using System.Net.Http.Headers;

namespace FoosballProLeague.Webserver.Service;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HttpClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        string baseAddress = _configuration["HttpClientSettings:BaseAddress"];
        if (!string.IsNullOrEmpty(baseAddress))
        {
            _httpClient.BaseAddress = new Uri(baseAddress);
        }

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