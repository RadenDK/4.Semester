using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface IDepartmentLogic
    {
        public List<DepartmentModel> GetDepartments();
    }
}
