using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public interface ICompanyLogic
{
    public List<CompanyModel> GetCompanies();
}