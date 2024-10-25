using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic;

public interface IDepartmentLogic
{
    public List<DepartmentModel> GetDepartments();
}