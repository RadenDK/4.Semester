using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.DatabaseAccess;

namespace FoosballProLeague.Api.BusinessLogic;

public class CompanyLogic : ICompanyLogic
{
    ICompanyDatabaseAccessor _companyDatabaseAccessor;
    
    public CompanyLogic(ICompanyDatabaseAccessor companyDatabaseAccessor)
    {
        _companyDatabaseAccessor = companyDatabaseAccessor;
    }
    public List<CompanyModel> GetCompanies()
    {
        return _companyDatabaseAccessor.GetCompanies();
    }

    public List<DepartmentModel> GetDepartments()
    {
        return _companyDatabaseAccessor.GetDepartments();
    }
}