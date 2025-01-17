using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface IRegistrationService
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);

    Task<List<CompanyModel>> GetCompaniesAsync();

    Task<List<DepartmentModel>> GetDepartments();
}