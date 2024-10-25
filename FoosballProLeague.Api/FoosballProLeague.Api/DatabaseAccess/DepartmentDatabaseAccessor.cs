using System.Data;
using Dapper;
using FoosballProLeague.Api.Models;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess;

public class DepartmentDatabaseAccessor : IDepartmentDatabaseAccessor
{
    private readonly string _connectionString;

    public DepartmentDatabaseAccessor(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DatabaseConnection");
    }
    
    public List<DepartmentModel> GetDepartments()
    {
        List<DepartmentModel> departments = new List<DepartmentModel>();
        string query = "SELECT id AS Id, name AS Name, company_id AS CompanyId FROM departments";

        using (IDbConnection connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            departments = connection.Query<DepartmentModel>(query).ToList();
        }

        return departments;
    }
}