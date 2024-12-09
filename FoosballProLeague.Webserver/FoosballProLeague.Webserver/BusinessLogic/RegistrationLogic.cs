using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;

namespace FoosballProLeague.Webserver.BusinessLogic;

public class RegistrationLogic : IRegistrationLogic
{
    private readonly IRegistrationService _registrationService;
    
    public RegistrationLogic(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }
    
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        return await _registrationService.SendUserToApi(newUser);
    }

    public async Task<List<CompanyModel>> GetCompaniesAsync()
    {
        return await _registrationService.GetCompaniesAsync();
    }

    public async Task<List<DepartmentModel>> GetDepartments()
    {
        return await _registrationService.GetDepartments();
    }
}