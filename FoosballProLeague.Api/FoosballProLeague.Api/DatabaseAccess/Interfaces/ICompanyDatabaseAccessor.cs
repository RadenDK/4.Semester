using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface ICompanyDatabaseAccessor
    {
        public List<CompanyModel> GetCompanies();

    }
}
