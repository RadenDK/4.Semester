using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic.Interfaces;

public interface IRegistrationLogic
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);

    Task<List<CompanyModel>> GetCompaniesAsync();

    Task<List<DepartmentModel>> GetDepartments();
}