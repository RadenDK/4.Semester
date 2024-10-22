using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public interface ICompanyLogic
{
    public List<CompanyModel> GetCompanies();
    public List<DepartmentModel> GetDepartments(int companyId);
}