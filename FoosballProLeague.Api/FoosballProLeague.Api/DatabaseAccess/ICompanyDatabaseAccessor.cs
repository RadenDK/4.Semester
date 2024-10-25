using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface ICompanyDatabaseAccessor
{
    public List<CompanyModel> GetCompanies();
    
}