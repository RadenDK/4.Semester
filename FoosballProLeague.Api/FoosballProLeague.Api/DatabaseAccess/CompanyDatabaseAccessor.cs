using System.Data;
using Dapper;
using FoosballProLeague.Api.Models;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess;

public class CompanyDatabaseAccessor : ICompanyDatabaseAccessor
{
    private readonly string _connectionString;

    public CompanyDatabaseAccessor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnection");
    }
    
    public List<CompanyModel> GetCompanies()
    {
        List<CompanyModel> companies = new List<CompanyModel>();
        string query = "SELECT id AS Id, name AS Name FROM companies";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            companies = connection.Query<CompanyModel>(query).ToList();
        }
        return companies;
    }
}