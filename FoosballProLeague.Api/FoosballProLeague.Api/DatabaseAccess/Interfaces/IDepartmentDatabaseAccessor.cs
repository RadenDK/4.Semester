using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.DatabaseAccess.Interfaces
{
    public interface IDepartmentDatabaseAccessor
    {
        public List<DepartmentModel> GetDepartments();
    }
}