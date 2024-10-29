using System.Data;
using Dapper;
using FoosballProLeague.Api.Models;
using Npgsql;

namespace FoosballProLeague.Api.DatabaseAccess;

public class DepartmentDatabaseAccessor : DatabaseAccessor, IDepartmentDatabaseAccessor
{

    public DepartmentDatabaseAccessor(IConfiguration configuration) : base(configuration)
    {
    }
    
    public List<DepartmentModel> GetDepartments()
    {
        List<DepartmentModel> departments = new List<DepartmentModel>();
        string query = "SELECT id, name, company_id FROM departments";

        using (IDbConnection connection = GetConnection())
        {
            connection.Open();
            departments = connection.Query<DepartmentModel>(query).ToList();
        }

        return departments;
    }
}