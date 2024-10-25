using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess;

public interface IDepartmentDatabaseAccessor
{
    public List<DepartmentModel> GetDepartments();
}