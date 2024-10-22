using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace FoosballProLeague.Webserver.Service;

public class RegistrationService : IRegistrationService
{
    private readonly IHttpClientService _httpClientService;
    
    public RegistrationService(IHttpClientService httpClientService)
    {
        _httpClientService = httpClientService;
    }
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        UserRegistrationModel user = new UserRegistrationModel
        {
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            Password = newUser.Password
        };
        
        string json = JsonSerializer.Serialize(user);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        
        return await _httpClientService.PostAsync("/User/user", content);
    }
    
    public async Task<List<CompanyModel>> GetCompaniesAsync()
    {
        HttpResponseMessage response = await _httpClientService.GetAsync("/Company/companies");
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CompanyModel>>(responseBody);
        }
        else
        {
            throw new Exception($"Could not get users. HTTP status code: {response.StatusCode}");
        }
    }

    public async Task<List<DepartmentModel>> GetDepartmentByCompanyId(int companyId)
    {
        var requestBody = new { companyId = companyId };
        StringContent content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _httpClientService.PostAsync("/Company/departments", content);
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DepartmentModel>>(responseBody);
        }
        else
        {
            throw new Exception($"Could not get department. HTTP status code: {response.StatusCode}");
        }
    }
}