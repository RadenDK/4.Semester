using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public class DepartmentLogic : IDepartmentLogic
{
    IDepartmentDatabaseAccessor _departmentDatabaseAccessor;
    
    public DepartmentLogic(IDepartmentDatabaseAccessor departmentDatabaseAccessor)
    {
        _departmentDatabaseAccessor = departmentDatabaseAccessor;
    }
    public List<DepartmentModel> GetDepartments()
    {
        return _departmentDatabaseAccessor.GetDepartments();
    }
}