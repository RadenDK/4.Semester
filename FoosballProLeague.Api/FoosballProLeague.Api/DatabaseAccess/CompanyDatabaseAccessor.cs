using Dapper;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models;
using System.Data;

namespace FoosballProLeague.Api.DatabaseAccess
{
    public class CompanyDatabaseAccessor : DatabaseAccessor, ICompanyDatabaseAccessor
    {

        public CompanyDatabaseAccessor(IConfiguration configuration) : base(configuration)
        {
        }

        public List<CompanyModel> GetCompanies()
        {
            List<CompanyModel> companies = new List<CompanyModel>();
            string query = "SELECT id, name FROM companies";

            using (IDbConnection connection = GetConnection())
            {
                connection.Open();
                companies = connection.Query<CompanyModel>(query).ToList();
            }
            return companies;
        }
    }
}
